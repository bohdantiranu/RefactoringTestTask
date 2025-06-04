# Issues list
1. Assert usage for data validation - Assert should be used only in tests, use if statement instead assert and throw relevant exception;
2. Syncronus requests to db - make async;
3. No separation between business and data layers - use repository pattern to create data layer(but I will simulate it by encapsulating db request logic in another method);
4. IDisposable objects are not disposed - use a using statement;
5. Direct passing of orderCode in sql query makes sql-injection possible - use a parametrized query;
6. Potential race condition between locks can lead to multiple requests to db -  use ConcurrentDictionary<string, Task<Order>>,  ConcurrentDictionary.GetOrAdd guarantees that the task will be created only once for a specific order code;
7. I think ApplicationException is not good in this case, it's not informative - maybe use some custom exception;
8. I think cache should be private;