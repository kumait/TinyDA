# TinyDA
*TinayDA is a very light .net data access library that simplifies accessing data over ADO.net*
***
The main idea behind TinyDA is using nameless paramaters in SQL statements, this makes it possible to create a simple API that is easy and fun to use.

TinyDA creates a thin layer over ADO.net to eleminate most if not all of boilerplate code. The design also promotes minimal invasive development where the library does not impose any requirements on the developer.
## Usage

### Field Mapping
One of the main problems to be tackled when accessing data in relational databases is how to map field names in tables to properties in classes. TinyDA uses the `IFieldMapper` interface to achieve that. It is shipped with 4 implementations.

1. `SimpleFieldMapper`: Maps field names to property names as they are with no change.
2. `UnderscoreFieldMapper`: Convert field names written in the UNDERSCORE_STYLE to pascal casing. for example if the field name is STUDENT_ID it gets mapped to a property named StudentId.
3. `AttributeFieldMapper`: Maps field names to property names by using the `Column` attribute which can be applied on properties to provide the name of the field the property is linked to.
4. `CustomFieldMapper`: is a basic implementation that allows the developer to map fields using a lambda function.

*The developer is free to provide any custom field mappers according to needs of none if the above mappers suffice.*

### Accessing Data
Accessing data is done through the `DataAccessor` class. `DataAccessor` requires an instance of `IDbConnection` and an instance of `IFieldMapper` that gets injected through the constructor. The injected `IFieldMapper` instance is used as a default field mapper when methods with no field mapper argument are used. If the overloaded constructor with no field mapper is used, an instance of `SimpleFieldMapper` is used as the default field mapper.

### Example
``` CS
IDataAccessor da = new DataAccessor(connection);

// insert user
int id = da.ExecuteScalar<int>("insert into user(name) output inserted.id values (?)", "Jack");

// update user
int count = da.ExecuteNonQuery("update user set name = ? where id = ?", "John", 22);

// get user with id 22
User u = da.getObject<User>("select * from User where id = ?", 22);

// get list of users
List<User> da.getList<User>("select * from User");

// get user name
string name = da.getValue<string>("select name from user where id = ?", 0, 22);

// get names of users (0 is field index)
List<string> names = da.getValues<string>("select name from user", 0);

// get users as a result of running a stored procedure
User u = da.getListSP<User>("GET_USERS", 22);
```
## Using mappers

####Database

``` SQL
CREATE TABLE STUDENT (
  STUDENT_ID int NOT NULL identity,
  STUDENT_NAME varchar(100) NOT NULL,
  COURSES int NOT NULL,
  BIRTH_DATE datetime NULL,
  PRIMARY KEY (STUDENT_ID)
)
GO

INSERT INTO STUDENT (STUDENT_NAME, COURSES, BIRTH_DATE) VALUES ('Jack', 8, '1990-12-01');
INSERT INTO STUDENT (STUDENT_NAME, COURSES, BIRTH_DATE) VALUES ('John', 4, '1986-08-06');
INSERT INTO STUDENT (STUDENT_NAME, COURSES, BIRTH_DATE) VALUES ('Sara', 16, '1988-08-14');
```
### Attribute Mapper

####Student Class

``` CS
public class Student
{
    [Column("STUDENT_ID")]
    public int? Id { get; set; }

    [Column("STUDENT_NAME")]
    public string Name { get; set; }

    [Column("COURSES")]
    public int? Courses { get; set; }

    [Column("BIRTH_DATE")]
    public DateTime BirthDate { get; set; }
}
```

###Accessing Data
``` CS
string SqlServerConnectionString = "Data Source=localhost;Initial Catalog=tinydatest;Integrated Security=True";
using (IDbConnection connection = new SqlConnection(SqlServerConnectionString))
{
    connection.Open();
    IDataAccessor da = new DataAccessor(connection);
    var mapper = new AttributeFieldMapper(typeof(Student));

    var student = da.GetObject<Student>("select * from student where STUDENT_NAME = ?", mapper, "Jack");

    int insertedId = da.ExecuteScalar<int>("insert into student(STUDENT_NAME, COURSES, BIRTH_DATE) output inserted.STUDENT_ID VALUES (?, ?, ?)", "Kumait", 16, DateTime.Now);

    int deletedCount = da.ExecuteNonQuery("delete from student where STUDENT_NAME like ?", "Kumait");

    var students = da.GetList<Student>("select * from student");

}

```


### Custom mapper

####Student Class
``` CS
public class Student4
{
    public int? Number { get; set; }
    public string Name { get; set; }
    public int? CourseCount { get; set; }
    public DateTime DateOfBirth { get; set; }
}
```

###Accessing Data

``` CS
IDataAccessor da = new DataAccessor(connection);

var mapper = new CustomFieldMapper((f) => {
    switch (f)
    {
        case "STUDENT_ID": return "Number";
        case "STUDENT_NAME": return "Name";
        case "COURSES": return "CourseCount";
        case "BIRTH_DATE": return "DateOfBirth";
        default: return null;
    }
});

Student4 student = da.GetObject<Student4>("select * from student where STUDENT_NAME = ?", mapper, "Jack");
List<Student3> students = da.GetList<Student3>("select * from student", mapper);
```

For more examples, please have a look at the [MapperTest.cs](https://github.com/kumait/TinyDA/blob/v1.1/TinyDA.Test/MapperTest.cs)
