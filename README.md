# BusterWood.Mapper
[![Build status](https://ci.appveyor.com/api/projects/status/vdlxdx8t62mfrrol/branch/master?svg=true)](https://ci.appveyor.com/project/busterwood/mapper/branch/master)

.NET composable mapping library for objects and data (System.Data).

Sort of a replacement for Dapper (150K) and Automapper (350K) but `Mapper` is *much smaller* at around 87K.

Performance is "good" as `Mapper` uses the DLR to create and JIT compile methods to do the mapping, and these methods are cached.

## Copying an object

`Mapper` contains an extension method for all objects called `Copy<T>()` which returns a *shallow* copy of the original object. The type being cloned *must* have a parameterless contructor, then all public properties and fields are copied.

`Mapper` can also clone sequences of object via the `CopyAll<T>()` extension which takes a `IEnumerable<T>` and returns an `IEnumerable<T>`.

To allow for customized copying the following overloads of `CopyAll<T>()` take an extra action to be performed on each copy:
* `CopyAll<T>(Func<T, T>)` calls the supplied function for each copied object 
* `CopyAll<T>(Func<T, T, int>)` calls the supplied function for each mapped object passing the zero based index of the object 

## Copying between different types

You can copy an object of one type to another type using the `Copy<TFrom,TTo>()` extension method.  The type being mapped *to* **must** have a parameterless contructor, then all readable public properties (and fields) of the source type are copied to properties (or fields) of the target type.  

`Mapper` can also copy sequences of objects via the `CopyAll<TFrom,TTo>()` extension which takes a `IEnumerable<TFrom>` and returns an `IEnumerable<TTo>`.  `CopyAll<TFrom,TTo>()` has overloads that allow the mapping to be customized:

* `CopyAll<TFrom,TTo>(Func<TFrom,TTo>)` calls the supplied function for each mapped object
* `CopyAll<TFrom,TTo>(Func<TFrom,TTo, int>)` calls the supplied function for each mapped object passing the zero based index of the object 

## Type compatibility

When copying data types must be compatible in *some sense*, the following lists the type compatibility rules:

| Source Type                                       | Target Type                                                                                                              |
|---------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| Any numeric type or enum                          | Any numeric type or any enum                                                                                             |
| `Nullable<T>` where T is any numeric type or enum | any numeric type or any enum. `default(T)` is used as the value if value is null                                         |
| `Nullable<T>` where T is any numeric type or enum | `Nullable<T>` where T is any numeric type or enum                                                                        |
| any type other                                    | type must match or be [assignable](https://msdn.microsoft.com/en-us/library/system.type.isassignablefrom(v=vs.110).aspx) |

## Name compatibility

For `Copy`, `CopyAll` and all the data mappings, the following rules apply when looking for the destination field or property to map to:

1. the source name (case insensitive)
2. if the name ends with 'ID' then try the name without 'ID'  (case insensitive)
3. if the name does *not* end with 'ID' then try the name with 'Id' suffix added (case insensitive)
4. the above names with underscores removed  (case insensitive)
5. the above names with the target class name prefix removed (case insensitive)

Note that the rules are following in the above sequence, and that rules 2 & 3 only apply when the data type of the field being mapped is a primative type, and enum, or a nullable<T> of those types.

For example, if the source name is `ORDER_ID` then the following names would be considered  (shown in perference order):

1. ORDER_ID
2. ORDER_
3. ORDERID
4. ORDER
5. ID     (* this will be considered when mapping from a DbDataReader to a type called `Order`)

Note: name comparison is *case insensitive*.

## ADO.NET DbConnection extensions

`Mapper` adds `Query()` and `Execute()` extension methods, as well as `...Async()` variants.

The `Query(string sql, object parameters = null)` extension executes the supplied SQL (with optional parameters) and returns a `DbDataReader`.

The `Execute(string sql, object parameters = null)` extension executes the supplied SQL (with optional parameters) but *just returns the number of rows affected*.

As a conveniance, if `Query()` and `Execute()` are called on a closed connection then `Mapper` will open the connection, use it, and close/dispose the connection afterwards.

## ADO.NET DbCommand methods

`Mapper` adds `AddParameters(object parameters)` extension method to `System.Data.Common.DbCommand`. `AddParameters` will add a `DbDataParameter` to the commands `Parameters` collection for each readable public property (and field) of `parameters`, setting the type and value.

## ADO.NET DbDataReader extensions

`Mapper` adds the following extension methods to `DbDataReader` (as returned by `Query()` and `Execute()`) to read and map the data:

* `Single<T>(this DbDataReader reader, ...)` reads one and only one `T`
* `SingleOrDefault<T>(this DbDataReader reader, ...)` reads one or zero one `T`
* `ToDictionary<TKey, TValue>(this DbDataReader reader, ...)` reads `TValue` items and creates a hash table with a unique `TKey` for each `TValue`
* `ToList<T>(this DbDataReader reader, ...)` reads a list of `T`
* `ToLookup<TKey, TValue>(this DbDataReader reader, ...)` reads `TValue` items but groups the items by key

Additional `...Async()` extension methods also exist.

`T` can be:
* a `class` with a parameterless constructor - in which case public set-able fields and properties are mapped
* a `struct` - again, public set-able fields and properties are mapped
* a single value, e.g. `long`, `string` or an enumeration
* a `Nullable<T>` of primative value (e.g. 'int') or an enumeration

Note that the above methods take an optional `Action<DbDataReader,T>` parameter that allow you to add custom mapping between the current record of the data reader and the mapped version of `T`.

## ADO.NET SqlDataRecord methods

`Mapper` adds a `ToTableType<T>()` extension method to `IEnumerable<T>` that convert it into a `IEnumerable<SqlDataRecord>` such that it can be passed as a [table valued parameter](https://msdn.microsoft.com/en-us/library/bb675163(v=vs.110).aspx) to SQL Server.

## Examples

Query returning a list:
```csharp
List<Order> list = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.ToList<Order>();
```

Asynchronously query returning a list:
```csharp
List<Order> list = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.ToListAsync<Order>();
```

Query returning a dictionary for a unqiue key:
```csharp
Dictionary<int, Order> byId = connection.Query("select * from dbo.[Order] where status = @Status", new { Status = 1 })
	.ToDictionary<Order>(order => order.Id);
```

Asynchronously query returning a dictionary for a unqiue key::
```csharp
Dictionary<int, Order> byId = await connection.QueryAsync("select * from dbo.[Order] where status = @Status", new { Status = 1 })
	.ToDictionaryAsync<Order>(order => order.Id);
```

Query returning `HashLookup` for a non-unqiue key:
```csharp
HashLookup<int, Order> byStatus = connection.Query("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) })
	.ToLookup<Order>(order => order.Status);
```

Asynchronously query returning `HashLookup` for a non-unqiue key:
```csharp
HashLookup<int, Order> byStatus = await connection.QueryAsync("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) })
	.ToLookupAsync<Order>(order => order.Status);
```

Query returning exactly one row:
```csharp
Order order = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Single<Order>();
```

Asynchronously query returning exactly one row:
```csharp
Order order = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.SingleAsync<Order>();
```

Query returning exactly one row of a primative type:
```csharp
int count = connection.Query("select count(*) from dbo.[Order] where order_type = @orderType", new { orderType = 3 })
	.Single<int>();
```

Query returning exactly zero or one rows:
```csharp
Order order = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.SingleOrDefault<Order>();
```

Asynchronously query returning zero or one rows:
```csharp
Order order = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.SingleOrDefaultAsync<Order>();
```

Query returning zero or one rows of a enum:
```csharp
OrderType? orderType = connection.Query("select order_type_id from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.SingleOrDefault<OrderType?>();
```

Call a stored procedure that does not return results set(s)
```csharp
int rowsChanged = connection.Execute("EXEC update_user_name @user_id=@id, @name=@name", new { id=123, name="fred" });
```

Asynchronously call a stored procedure that does not return results set(s)
```csharp
int rowsChanged = await connection.ExecuteAsync("EXEC update_user_name @user_id=@id, @name=@name", new { id=123, name="fred" });
```
