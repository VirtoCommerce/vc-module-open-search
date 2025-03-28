using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VirtoCommerce.OpenSearch.Tests
{
    public class PriorityTestCaseOrderer : ITestCaseOrderer
    {
        public const string TypeName = "VirtoCommerce.OpenSearch.Tests.PriorityTestCaseOrderer";
        public const string AssemblyName = "VirtoCommerce.OpenSearch.Tests";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            return testCases.OrderByDescending(GetPriority);
        }

        private static int GetPriority<TTestCase>(TTestCase testCase)
            where TTestCase : ITestCase
        {
            // Order the test based on the attribute.
            var attr = testCase.TestMethod.Method
                .ToRuntimeMethod()
                .GetCustomAttribute<PriorityAttribute>();

            return attr?.Priority ?? 0;
        }
    }
}
