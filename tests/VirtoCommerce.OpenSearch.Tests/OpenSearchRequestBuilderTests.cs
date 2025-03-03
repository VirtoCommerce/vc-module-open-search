using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Moq;
using OpenSearch.Client;
using VirtoCommerce.OpenSearch.Data;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.OpenSearch.Tests
{
    [Trait("Category", "Unit")]
    public class OpenSearchRequestBuilderTests
    {
        private readonly OpenSearchRequestBuilderTestProxy _testClass = new();
        private readonly Fixture _fixture = new();

        [Theory]
        [InlineData("0", "false")]
        [InlineData("1", "true")]
        [InlineData("true", "true")]
        [InlineData("false", "false")]
        [InlineData("tRuE", "true")]
        [InlineData("FaLsE", "false")]
        public void CreateTermFilter_BooleanAggregate_ShouldCreateCorrectValues(string value, string convertedValue)
        {
            // Arrange
            var fieldName = _fixture.Create<string>();

            var termFilter = new TermFilter
            {
                Values = new[] { value },
                FieldName = fieldName
            };

            var booleanPropertyMock = new Mock<IProperty>();
            booleanPropertyMock
                .SetupGet(x => x.Type)
                .Returns("boolean");

            var availableFields = new Properties<IProperties>(new Dictionary<PropertyName, IProperty>
            {
                { fieldName, booleanPropertyMock.Object }
            });

            // Act
            var result = _testClass.CreateTermFilterProxy(termFilter, availableFields) as IQueryContainer;

            // Assert
            result.Terms.Terms.Should().Contain(convertedValue);
        }
    }

    public class OpenSearchRequestBuilderTestProxy : OpenSearchRequestBuilder
    {
        public QueryContainer CreateTermFilterProxy(TermFilter termFilter, Properties<IProperties> availableFields)
        {
            return base.CreateTermFilter(termFilter, availableFields);
        }
    }
}
