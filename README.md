# mapper
.NET composable mapping library for objects and data (System.Data).

Sort of a replacement for Dapper (150K) and Automapper (350K) but `Mapper` is *much smaller* at around 75K.

Performance is "good" as `Mapper` uses the DLR to create and JIT compile methods to do the mapping, and these methods are cached.

## Cloning

`Mapper` contains an extension method for all objects called `Clone` which performs a *shallow* clone. The type being cloned *must* have a parameterless contructor, then all public properties and fields are copied.

`Mapper` can also clone sequences of object via the `CloneSome<T>()` extension which takes a `IEnumerable<T>` and returns an `IEnumerable<T>`.

## Mapping

You can copy an object of one type to another type using the `Map<TFrom,TTo>()` extension method.  The type being mapped to *must* have a parameterless contructor, then all readable public properties (and fields) of the source type are copied to properties (or fields) of the target type.  

`Mapper` can also copy sequences of objects via the `MapSome<TFrom,TTo>()` extension which takes a `IEnumerable<TFrom>` and returns an `IEnumerable<TTo>`.  `MapSome<TFrom,TTo>()` has overloads that allow the mapping to be customized:

* `MapSome<TFrom,TTo>(Func<TFrom,TTo>)` calls the supplied function for each mapped object
* `MapSome<TFrom,TTo>(Func<TFrom,TTo, int>)` calls the supplied function for each mapped object passing the zero based index of the object 

A property (or field) types must be compatible in some sense, the following list the type compatibility rules:

| Source Type                                       | Target Type                                                                                                              |
|---------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| Any numeric type or enum                          | Any numeric type or any enum                                                                                             |
| `Nullable<T>` where T is any numeric type or enum | any numeric type or any enum. `default(T)` is used as the value if value is null                                         |
| `Nullable<T>` where T is any numeric type or enum | `Nullable<T>` where T is any numeric type or enum                                                                        |
| any type other                                    | type must match or be [assignable](https://msdn.microsoft.com/en-us/library/system.type.isassignablefrom(v=vs.110).aspx) |

## Name compatibility

For `Map`, `MapSome` and all the data mappings, the following rules apply when looking for candidate names:

1. the source name (case insensitive)
2. if the name ends with 'ID' then try the name without 'ID'  (case insensitive)
3. the name above names with underscores removed  (case insensitive)

For example, if the source name is `ORDER_ID` then the following names would be considered  (shown in perference order):

1. ORDER_ID
2. ORDER_
3. ORDERID
4. ORDER

Note: name comparison is *case insensitive*.

## Data extensions

The headline examples is much like Dapper, but methods have strongly typed return values:

Select a list:
```
List<Order> list = connection.Execute<Order>("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 }).ToList();
```

Select a dictionary keyed by the primary key:
```
Dictionary<int, Order> byId = connection.Execute<Order>("select * from dbo.[Order] where status = @Status", new { Status = 1 }).ToDictionary(order => order.Id);
```

Select a key to multiple value `HashLookup`:
```
HasLookup<int, Order> byStatus = connection.Execute<Order>("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) }).ToLookup(order => order.Status);
```

## Data Composability

`Mapper` has a series of extension methods for ADO.Net types:

### DataSequence methods
`System.Data.Common.DbDataReader` has the following extension methods:

* `Single<T>()` for reading exactly one row
* `SingleOrDefault<T>()` for reading zero or one rows
* `ToList<T>()` for reading all records into a `List<T>`
* `ToDictinary<TKey,TValue>(Func<TKey,TValue> keyFunc)` for reading all records into a `Dictinary<TKey,TValue>` using the supplied function to get work out the key.  Note that the key must be unique.
* `ToLookup<TKey,TValue>(Func<TKey,TValue> keyFunc)` for reading all records into a `HashLookup<TKey,TValue>` using the supplied function to get work out the key.  Each key may have multiple values.

Additional `...Async` methods exist for reading data using tasks.

## DbCommand methods

`Mapper` adds `AddParameters(object parameters)` extension method to `System.Data.Common.DbCommand`. `AddParameters` will add a `DbDataParameter` to the commands `Parameters` collection for each readable public property (and field) of `parameters`, setting the type and value.

For convenience `Mapper` adds the following extension method to `System.Data.Common.DbCommand`:

* `Execute<T>()` for exeucting the command, returns a `DataSequence<T>`
* `ExecuteAsync<T>()` for exeucting the command asynchronously, returns a `DataSequence<T>`
* `ExecuteScalar<T>()` for exeucting the command, returning the value of the first column of the first row
* `ExecuteScalarAsync<T>()` for exeucting the command asynchronously, returning the value of the first column of the first row

### DbConnetion methods

For convenience `Mapper` adds the following extension method to `System.Data.Common.DbConnection`:

* `ExecuteNonQuery(string sql, object parameters)` for executing database commands that do not return result sets
* `QuerySingle<T>()` for executing the command and reading exactly one row
* `QuerySingleOrDefault<T>()` for executing the command and reading zero or one rows
* `QueryList<T>()` for executing the command and reading all records into a `List<T>`
* `QueryDictinary<TKey,TValue>(Func<TKey,TValue> keyFunc)` for executing the command and reading all records into a `Dictinary<TKey,TValue>` using the supplied function to get work out the key.  Note that the key must be unique.
* `QueryLookup<TKey,TValue>(Func<TKey,TValue> keyFunc)` for executing the command and reading all records into a `HashLookup<TKey,TValue>` using the supplied function to get work out the key.  Each key may have multiple values.
* `QueryScalar<T>()` for exeucting the command and reading the first value of the first row

Additional `...Async` methods exist for executing commands using tasks.

### SqlDataRecord methods

`Mapper` has a extension method `ToTableType<T>()` for converting a source `IEnumerable<T>` into an `IEnumerable<SqlDataRecord>` such that it can be passed as a [table valued parameter](https://msdn.microsoft.com/en-us/library/bb675163(v=vs.110).aspx) to SQL Server.
