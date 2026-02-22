using Polchan.Application.Pagination;

namespace Polchan.UnitTests.Application.Pagination;

public class PaginationTests
{
    private static readonly int _validPageNumber = 2;
    private static readonly int _validPageSize = 20;
    private static readonly string _validColumn = "name";
    private static readonly bool _validAscending = false;

    [Fact]
    public void PaginationQuery_ShouldParse_GivenProperString()
    {
        // Arrange
        var queryString = $"pageNumber:{_validPageNumber};pageSize:{_validPageSize};orderBy:{{column:{_validColumn},ascending:{_validAscending}}}";

        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        Assert.Equal(_validPageNumber, paginationQuery.PageNumber);
        Assert.Equal(_validPageSize, paginationQuery.PageSize);
        Assert.NotNull(paginationQuery.OrderBy);
        Assert.Equal(_validColumn, paginationQuery.OrderBy.Column);
        Assert.Equal(_validAscending, paginationQuery.OrderBy.Ascending);
    }

    [Fact]
    public void PaginationQuery_ShouldParse_GivenMinimumRequiredParameters()
    {
        // Arrange
        var queryString = $"pageNumber:{_validPageNumber};pageSize:{_validPageSize}";

        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        Assert.Equal(_validPageNumber, paginationQuery.PageNumber);
        Assert.Equal(_validPageSize, paginationQuery.PageSize);
        Assert.Null(paginationQuery.OrderBy);
    }

    [Fact]
    public void PaginationQuery_ShouldParse_GivenUntrimmedParameters()
    {
        // Arrange
        var queryString = $"pageNumber: {_validPageNumber} ; pageSize: {_validPageSize}";

        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        Assert.Equal(_validPageNumber, paginationQuery.PageNumber);
        Assert.Equal(_validPageSize, paginationQuery.PageSize);
        Assert.Null(paginationQuery.OrderBy);
    }

    [Theory]
    [InlineData("pageNumber:0;pageSize:20")]
    [InlineData("pageNumber:-1;pageSize:20")]
    [InlineData("pageNumber:2;pageSize:0")]
    [InlineData("pageNumber:2;pageSize:-5")]
    public void PaginationQuery_ShouldParse_GivenZeroOrNegativePageNumberAndPageSize(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        // Note: The actual validation of ranges happens in the ToPaginatedListAsync method
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;unknownParam:value")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:true};extraParam:test")]
    public void PaginationQuery_ShouldParse_GivenExtraUnknownParameters(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        Assert.Equal(2, paginationQuery.PageNumber);
        Assert.Equal(20, paginationQuery.PageSize);
    }

    [Fact]
    public void PaginationQuery_ShouldParse_GivenCaseInsensitiveParameters()
    {
        // Arrange
        var queryString = $"Pagenumber:{_validPageNumber};PAGESIZE:{_validPageSize};orderby:{{Column:{_validColumn},AScENDinG:{_validAscending}}}";

        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.True(parseResult);
        Assert.NotNull(paginationQuery);
        Assert.Equal(_validPageNumber, paginationQuery.PageNumber);
        Assert.Equal(_validPageSize, paginationQuery.PageSize);
        Assert.NotNull(paginationQuery.OrderBy);
        Assert.Equal(_validColumn, paginationQuery.OrderBy.Column);
        Assert.Equal(_validAscending, paginationQuery.OrderBy.Ascending);
    }

    [Theory]
    [InlineData("pageNumber:2")]
    [InlineData("pageSize:20")]
    [InlineData("orderBy:{column:name,ascending:true}")]
    [InlineData("pageNumber:2;orderBy:{column:name,ascending:true}")]
    [InlineData("pageSize:20;orderBy:{column:name,ascending:true}")]
    public void PaginationQuery_ShouldFailToParse_GivenMissingRequiredParameters(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize")]
    [InlineData("pageNumber:2;pageSize:")]
    [InlineData("pageNumber:;pageSize:20")]
    [InlineData("pageNumber;pageSize:20")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:")]
    [InlineData("pageNumber:2;pageSize:20;orderBy")]
    public void PaginationQuery_ShouldFailToParse_GivenEmptyParameters(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:false};pageNumber:3")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:false};pageSize:30")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:false};orderBy:{column:age,ascending:true}")]
    public void PaginationQuery_ShouldFailToParse_GivenDuplicateParameters(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:abc;pageSize:20")]
    [InlineData("pageNumber:2def;pageSize:20")]
    [InlineData("pageNumber:true;pageSize:20")]
    public void PaginationQuery_ShouldFailToParse_GivenNonNumericPageNumber(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:abc")]
    [InlineData("pageNumber:2;pageSize:20def")]
    [InlineData("pageNumber:2;pageSize:true")]
    public void PaginationQuery_ShouldFailToParse_GivenNonNumericPageSize(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{ascending:true}")]
    public void PaginationQuery_ShouldFailToParse_GivenMissingOrderByProperties(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:,ascending:true}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column,ascending:true}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending}")]
    public void PaginationQuery_ShouldFailToParse_GivenEmptyOrderByProperties(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:false,column:age}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:false,ascending:true}")]
    public void PaginationQuery_ShouldFailToParse_GivenDuplicateOrderByProperties(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Fact]
    public void PaginationQuery_ShouldFailToParse_GivenNonObjectOrderBy()
    {
        // Arrange
        var queryString = $"pageNumber:{_validPageNumber};pageSize:{_validPageSize};orderBy:column:name,ascending:true";

        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:0}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:1}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:-1}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:2}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:abc}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:name,ascending:truedef}")]
    public void PaginationQuery_ShouldFailToParse_GivenNonBooleanAscending(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }

    [Theory]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column:,ascending:true}")]
    [InlineData("pageNumber:2;pageSize:20;orderBy:{column: ,ascending:true}")]
    public void PaginationQuery_ShouldFailToParse_GivenEmptyColumnName(string queryString)
    {
        // Act
        var parseResult = PaginationQuery.TryParse(queryString, null, out var paginationQuery);

        // Assert
        Assert.False(parseResult);
        Assert.Null(paginationQuery);
    }
}
