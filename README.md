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
5. ID     (* this will be consider when mapping from a DbDataReader to a type called `Order`)

Note: name comparison is *case insensitive*.

## Data extensions

ADO.NET connections have `Query()` and `Execute()` extension methods added.

The `Query()` methods return a `DbDataReader`. You then use the `Read<T>()` extension to create read object from the `DbDataReader`, the `Read<T>()` method takes an optional `Action<DbDataReader, T>` parameter that allow the mapping to be customized.
Finally use the extension methods to convert to Lists, Dictionary and Lookups, as well as allow custom collection creation by being `IEnumerable<T>`.

Query returning a list:
```csharp
List<Order> list = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToList();
```

Asynchronously query returning a list:
```csharp
List<Order> list = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToListAsync();
```

Query returning a dictionary for a unqiue key:
```csharp
Dictionary<int, Order> byId = connection.Query("select * from dbo.[Order] where status = @Status", new { Status = 1 })
	.Read<Order>().ToDictionary(order => order.Id);
```

Asynchronously query returning a dictionary for a unqiue key::
```csharp
Dictionary<int, Order> byId = await connection.QueryAsync("select * from dbo.[Order] where status = @Status", new { Status = 1 })
	.Read<Order>().ToDictionaryAsync(order => order.Id);
```

Query returning `HashLookup` for a non-unqiue key:
```csharp
HashLookup<int, Order> byStatus = connection.Query("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) })
	.Read<Order>().ToLookup(order => order.Status);
```

Asynchronously query returning `HashLookup` for a non-unqiue key:
```csharp
HashLookup<int, Order> byStatus = await connection.QueryAsync("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) })
	.Read<Order>().ToLookupAsync(order => order.Status);
```

Query returning exactly one row:
```csharp
Order order = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToSingle();
```

Asynchronously query returning exactly one row:
```csharp
Order order = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToSingleAsync();
```

Query returning exactly one row of a primative type:
```csharp
int count = connection.Query("select count(*) from dbo.[Order] where order_type = @orderType", new { orderType = 3 })
	.Read<int>().ToSingle();
```

Query returning exactly zero or one rows:
```csharp
Order order = connection.Query("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToSingleOrDefault();
```

Asynchronously query returning zero or one rows:
```csharp
Order order = await connection.QueryAsync("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<Order>().ToSingleOrDefaultAsync();
```

Query returning zero or one rows of a enum:
```csharp
OrderType? orderType = connection.Query("select order_type_id from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 })
	.Read<OrderType?>().ToSingleOrDefault();
```

Call a stored procedure that does not return results set(s)
```csharp
int rowsChanged = connection.Execute("EXEC update_user_name @user_id=@id, @name=@name", new { id=123, name="fred" });
```

Asynchronously call a stored procedure that does not return results set(s)
```csharp
int rowsChanged = await connection.ExecuteAsync("EXEC update_user_name @user_id=@id, @name=@name", new { id=123, name="fred" });
```

## Data Composability

`Mapper` has a series of extension methods for ADO.Net types:

`System.Data.Common.DbDataReader` has the following extension method:

* `Read<T>` which return a `DataSequence<T>`

### DataSequence<T> methods
`System.Data.Common.DbDataReader` has the following extension methods:

* `Single<T>()` for reading exactly one row
* `SingleOrDefault<T>()` for reading zero or one rows
* `ToList<T>()` for reading all records into a `List<T>`
* `ToDictinary<TKey,TValue>(Func<TKey,TValue> keyFunc)` for reading all records into a `Dictinary<TKey,TValue>` using the supplied function to get work out the key.  Note that the key must be unique.
* `ToLookup<TKey,TValue>(Func<TKey,TValue> keyFunc)` for reading all records into a `HashLookup<TKey,TValue>` using the supplied function to get work out the key.  Each key may have multiple values.

Additional `...Async` methods exist for reading data using tasks.

## DbCommand methods

`Mapper` adds `AddParameters(object parameters)` extension method to `System.Data.Common.DbCommand`. `AddParameters` will add a `DbDataParameter` to the commands `Parameters` collection for each readable public property (and field) of `parameters`, setting the type and value.

### DbConnetion methods

For convenience `Mapper` adds the following extension method to `System.Data.Common.DbConnection`:

* `Execute()` for runinng a command that returns no data
* `ExecuteAsync()` for asynchronously runinng a command that returns no data
* `Query()` for running a command, returns a `DbDataReader`
* `QueryAsync()` for running a command asynchronously, returns a `Task<DbDataReader>`

### SqlDataRecord methods

`Mapper` has a extension method `ToTableType<T>()` for converting a source `IEnumerable<T>` into an `IEnumerable<SqlDataRecord>` such that it can be passed as a [table valued parameter](https://msdn.microsoft.com/en-us/library/bb675163(v=vs.110).aspx) to SQL Server.
