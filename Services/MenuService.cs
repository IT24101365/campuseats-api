
using CampusEats.Api.Dtos;
using CampusEats.Api.Models;
using Npgsql;

namespace CampusEats.Api.Services;

public class MenuService : IMenuService
{
    private readonly DatabaseService _database;

    public MenuService(DatabaseService database)
    {
        _database = database;
    }

    public IEnumerable<MenuItemDto> GetAll()
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "SELECT id, name, price, category, available FROM menu_items ORDER BY id",
            connection);

        using var reader = command.ExecuteReader();
        var items = new List<MenuItemDto>();

        while (reader.Read())
        {
            items.Add(new MenuItemDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetString(3),
                reader.GetBoolean(4)));
        }

        return items;
    }

    public MenuItemDto? GetById(int id)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "SELECT id, name, price, category, available FROM menu_items WHERE id = @id",
            connection);
        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new MenuItemDto(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetDecimal(2),
            reader.GetString(3),
            reader.GetBoolean(4));
    }

    public MenuItemDto Create(CreateMenuItemDto dto)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            """
            INSERT INTO menu_items (name, price, category, available)
            VALUES (@name, @price, @category, TRUE)
            RETURNING id, name, price, category, available
            """,
            connection);

        command.Parameters.AddWithValue("name", dto.Name);
        command.Parameters.AddWithValue("price", dto.Price);
        command.Parameters.AddWithValue("category", dto.Category);

        using var reader = command.ExecuteReader();
        reader.Read();

        return new MenuItemDto(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetDecimal(2),
            reader.GetString(3),
            reader.GetBoolean(4));
    }

    public bool Update(int id, CreateMenuItemDto dto)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            """
            UPDATE menu_items
            SET name = @name, price = @price, category = @category
            WHERE id = @id
            """,
            connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("name", dto.Name);
        command.Parameters.AddWithValue("price", dto.Price);
        command.Parameters.AddWithValue("category", dto.Category);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = _database.CreateConnection();
        connection.Open();

        using var command = new NpgsqlCommand(
            "DELETE FROM menu_items WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("id", id);

        return command.ExecuteNonQuery() > 0;
    }
}
