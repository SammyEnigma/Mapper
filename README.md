# mapper
.NET composable mapping library for objects and data (System.Data)

## Cloning

`Mapper` contains an extension method for all objects called `Clone` which performs a *shallow* clone. The object being clone *must* have a parameterless contructor, then all public properties and fields are copied.

## Mapping

You can copy an object of one type to another type using the `Map<TFrom,TTo>()` extension method.

## Data extensions

The headline examples is much like Dapper, but methods are strongly typed:

Select a list:
```
List<Order> list = connection.QueryList<Order>("select * from dbo.[Order] where order_id = @OrderId", new { OrderId = 123 });
```

Select a dictionary keyed by the primary key
```
Dictionary<int, Order> list = connection.QueryDictionary<int, Order>("select * from dbo.[Order] where status = @Status", new { Status = 1 }, order => order.Id);
```

Select a key to multiple value `ILookup`
```
ILookup<int, Order> list = connection.QueryLookup<int, Order>("select * from dbo.[Order] where order_date > @OrderDate", new { OrderDate = new DateTime(2016, 8, 1) }, order => order.Status);
```

## Composability
TODO: `IDataReader` extensions
TODO: `IDataCommand` extensions
TODO: `SqlDataRecord` extensions
