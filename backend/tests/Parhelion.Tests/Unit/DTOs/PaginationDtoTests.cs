using Parhelion.Application.DTOs.Common;
using Xunit;

namespace Parhelion.Tests.Unit.DTOs;

/// <summary>
/// Tests para DTOs de paginación.
/// </summary>
public class PaginationDtoTests
{
    [Fact]
    public void PagedRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new PagedRequest();

        // Assert
        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
        Assert.True(request.ActiveOnly);
        Assert.Equal(0, request.Skip);
    }

    [Fact]
    public void PagedRequest_PageLessThanOne_SetsToOne()
    {
        // Arrange
        var request = new PagedRequest { Page = -5 };

        // Assert
        Assert.Equal(1, request.Page);
    }

    [Fact]
    public void PagedRequest_PageSizeExceedsMax_CapsAtMax()
    {
        // Arrange
        var request = new PagedRequest { PageSize = 500 };

        // Assert
        Assert.Equal(100, request.PageSize);
    }

    [Fact]
    public void PagedRequest_Skip_CalculatesCorrectly()
    {
        // Arrange
        var request = new PagedRequest { Page = 3, PageSize = 10 };

        // Assert
        Assert.Equal(20, request.Skip); // (3-1) * 10 = 20
    }

    [Fact]
    public void PagedResult_Empty_ReturnsEmptyResult()
    {
        // Arrange & Act
        var result = PagedResult<string>.Empty(2, 25);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(25, result.PageSize);
        Assert.Equal(0, result.TotalPages);
        Assert.True(result.HasPreviousPage);  // Page 2 always has previous
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_WithItems_CalculatesMetadata()
    {
        // Arrange
        var items = new[] { "a", "b", "c" };
        
        // Act
        var result = new PagedResult<string>(items, totalCount: 25, page: 2, pageSize: 10);

        // Assert
        Assert.Equal(3, result.Items.Count());
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages); // 25 / 10 = 2.5 -> 3
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_FirstPage_HasNoPreviousPage()
    {
        // Arrange & Act
        var result = new PagedResult<int>(new[] { 1, 2, 3 }, 30, page: 1, pageSize: 10);

        // Assert
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_LastPage_HasNoNextPage()
    {
        // Arrange & Act
        var result = new PagedResult<int>(new[] { 1, 2, 3 }, 23, page: 3, pageSize: 10);

        // Assert
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void OperationResult_Ok_SetsSuccessTrue()
    {
        // Arrange & Act
        var result = OperationResult.Ok("Test message");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Test message", result.Message);
    }

    [Fact]
    public void OperationResult_Fail_SetsSuccessFalse()
    {
        // Arrange & Act
        var result = OperationResult.Fail("Error message");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error message", result.Message);
    }

    [Fact]
    public void OperationResultGeneric_Ok_IncludesData()
    {
        // Arrange
        var data = new { Name = "Test", Value = 42 };

        // Act
        var result = OperationResult<object>.Ok(data, "Success");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Success", result.Message);
    }
}
