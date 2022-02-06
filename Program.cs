using CursoDapper.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CursoDapper
{
    class Program
    {
        static void Main(string[] args)
        {
            const string connectionString = "Server=localhost,1433;Database=balta;User ID=sa;Password=#12345678;TrustServerCertificate=True";

            using (var connection = new SqlConnection(connectionString))
            {
                // CreateCategory(connection);
                // UpdateCategory(connection);
                // DeleteCategory(connection);
                // ListCategories(connection);
                // GetCategory(connection);
                // ExecuteProcedure(connection);
                // ExecuteReadProcedure(connection);
                // ExecuteScalar(connection);
                // ReadView(connection);
                // OneToOne(connection);
                // OneToMany(connection);
                // QueryMultiple(connection);
                // SelectIn(connection);
                // Like(connection, "front");
            }
        }

        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>("SELECT [Id], [Title] FROM [Category]");
            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }

        static void GetCategory(SqlConnection connection)
        {
            var category = connection
                .QueryFirstOrDefault<Category>(
                    "SELECT TOP 1 [Id], [Title] FROM [Category] WHERE [Id]=@id",
                    new
                    {
                        id = "af3407aa-11ae-4621-a2ef-2028b85507c4"
                    });
            Console.WriteLine($"{category.Id} - {category.Title}");

        }

        static void CreateCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"INSERT INTO 
                    [Category] 
                VALUES(
                    @Id, 
                    @Title, 
                    @Url, 
                    @Summary, 
                    @Order, 
                    @Description, 
                    @Featured)";

            var rows = connection.Execute(insertSql, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });
            Console.WriteLine($"{rows} linhas inseridas");
        }

        static void UpdateCategory(SqlConnection connection)
        {
            var updateQuery = "UPDATE [Category] SET [Title]=@title WHERE [Id]=@id";
            var rows = connection.Execute(updateQuery, new
            {
                id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4"),
                title = "Frontend 2021"
            });

            Console.WriteLine($"{rows} registros atualizadas");
        }

        static void DeleteCategory(SqlConnection connection)
        {
            var deleteQuery = "DELETE [Category] WHERE [Id]=@id";
            var rows = connection.Execute(deleteQuery, new
            {
                id = new Guid("ea8059a2-e679-4e74-99b5-e4f0b310fe6f"),
            });

            Console.WriteLine($"{rows} registros excluídos");
        }

        static void ExecuteProcedure(SqlConnection connection)
        {
            var procedure = "[spDeleteStudent]";
            var pars = new { StudentId = "" }; // Criar student via insert no banco e adicionar o ID dele aqui.
            var affectedRows = connection.Execute(
                procedure,
                pars,
                commandType: System.Data.CommandType.StoredProcedure
            );
            Console.WriteLine($"{affectedRows} linhas afetadas");
        }

        static void ExecuteReadProcedure(SqlConnection connection)
        {
            var procedure = "[spGetCoursesBycategory]";
            var pars = new { CategoryId = "af3407aa-11ae-4621-a2ef-2028b85507c4" };
            var courses = connection.Query(
                procedure,
                pars,
                commandType: System.Data.CommandType.StoredProcedure
            );

            foreach (var item in courses)
            {
                Console.WriteLine(item.id);
            }
        }

        static void ExecuteScalar(SqlConnection connection)
        {
            var category = new Category();
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"
                INSERT INTO 
                    [Category] 
                OUTPUT inserted.[Id]
                VALUES(
                    NEWID(), 
                    @Title, 
                    @Url, 
                    @Summary, 
                    @Order, 
                    @Description, 
                    @Featured) 
                ";

            var id = connection.ExecuteScalar<Guid>(insertSql, new
            {
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });
            Console.WriteLine($"A categoria inserida foi: {id}");
        }

        static void ReadView(SqlConnection connection)
        {
            var sql = "SELECT * FROM [vwCourses]";
            var courses = connection.Query(sql);

            foreach (var item in courses)
            {
                Console.WriteLine($"{item.Id} = {item.Title}");
            }
        }

        static void OneToOne(SqlConnection connection)
        {
            var sql = @"
                SELECT 
                    * 
                FROM 
                    [CareerItem] 
                INNER JOIN 
                    [Course] ON [CareerItem].[CourseId] = [Course].[Id]";

            var items = connection.Query<CareerItem, Course, CareerItem>(
                sql,
                (careerItem, course) =>
                {
                    careerItem.Course = course;
                    return careerItem;
                }, splitOn: "Id");

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - Curso: {item.Course.Title}");
            }
        }

        static void OneToMany(SqlConnection connection)
        {
            var sql = @"
                SELECT
                    [Career].[Id],
                    [Career].[Title],
                    [CareerItem].[CareerId],
                    [CareerItem].[Title]
                FROM
                    [Career]
                INNER JOIN
                    [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
                ORDER BY
                    [Career].[Title]";

            var careers = new List<Career>();
            var items = connection.Query<Career, CareerItem, Career>(
                sql,
                (career, item) =>
                {
                    var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
                    if (car == null)
                    {
                        car = career;
                        car.Items.Add(item);
                        careers.Add(car);
                    }
                    else
                    {
                        car.Items.Add(item);
                    }
                    return career;
                }, splitOn: "CareerId");

            foreach (var career in careers)
            {
                Console.WriteLine($"{career.Title}");
                foreach (var item in career.Items)
                {
                    Console.WriteLine($" - {item.Title}");
                }
            }
        }

        static void QueryMultiple(SqlConnection connection)
        {
            var query = "SELECT * FROM [Category]; SELECT * FROM [Course]";

            using (var multi = connection.QueryMultiple(query))
            {
                var categories = multi.Read<Category>();
                var courses = multi.Read<Course>();

                foreach (var item in categories)
                {
                    Console.WriteLine(item.Title);
                }

                foreach (var item in courses)
                {
                    Console.WriteLine(item.Title);
                }
            }
        }

        static void SelectIn(SqlConnection connection)
        {
            var query = @"select * from Career where [Id] IN @Id";

            var items = connection.Query<Career>(query, new
            {
                Id = new[]{
                    "4327ac7e-963b-4893-9f31-9a3b28a4e72b",
                    "e6730d1c-6870-4df3-ae68-438624e04c72"
                }
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void Like(SqlConnection connection, string term)
        {
            var query = @"SELECT * FROM [Course] WHERE [Title] LIKE @exp";

            var items = connection.Query<Course>(query, new
            {
                exp = $"%{term}%"
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void Transaction(SqlConnection connection)
        {
            var category = new Category();
            category.Id = Guid.NewGuid();
            category.Title = "Minha categoria que não";
            category.Url = "amazon";
            category.Description = "Categoria destinada a serviços do AWS";
            category.Order = 8;
            category.Summary = "AWS Cloud";
            category.Featured = false;

            var insertSql = @"INSERT INTO 
                    [Category] 
                VALUES(
                    @Id, 
                    @Title, 
                    @Url, 
                    @Summary, 
                    @Order, 
                    @Description, 
                    @Featured)";

            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                var rows = connection.Execute(insertSql, new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured
                }, transaction);

                transaction.Commit();
                // transaction.Rollback();

                Console.WriteLine($"{rows} linhas inseridas");
            }
        }
    }
}