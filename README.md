# TinyDA
*TinayDA is a very light .net data access library for SQL Server*
***
The main idea behind TinyDA is using nameless paramaters in SQL statements, this makes it possible to create a simple API that is easy and fun to use.
## Usage

``` CS
IDataAccessor da = new DataAccessor(connection);

// insert user
int id = da.ExecuteScalar<int>("insert into u(name) output inserted.id values (?)", "Jack");

// update user
int count = da.ExecuteUpdate("update user set name = ? where id = ?", "John", 22);

// get user with id 22
User u = da.getObject<User>("select * from User where id = ?", 22);

// get list of users
List<User> da.getList<User>("select * from User");

// get user name
string name = da.getValue<string>("select name from user", 0);

// get names of users (0 is field index)
List<string> names = da.getValues<string>("select name from user", 0);

// get users as a result of running a stored procedure
User u = da.getListSP<User>("GET_USERS", 22);



```
