using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Selenium_xunit_template.Attributes
{
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; set; }
        public TestPriorityAttribute(int Priority)
        {
            this.Priority = Priority;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestDependencyAttribute : Attribute
    {
        public TestDependencyAttribute(string methodDependency)
        {
            this.MethodDependency = methodDependency;
        }

        public string MethodDependency { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestCollectionDependencyAttribute : Attribute
    {
        public Type Dependency { get; }

        public TestCollectionDependencyAttribute(Type dependency)
        {
            this.Dependency = dependency;
        }
    }

    public class PriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (TTestCase testCase in testCases)
            {
                int priority = 0;

                foreach (IAttributeInfo attr in testCase.TestMethod.Method.GetCustomAttributes((typeof(TestPriorityAttribute).AssemblyQualifiedName)))
                {
                    priority = attr.GetNamedArgument<int>("Priority");
                }

                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
            {
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                foreach (TTestCase testCase in list)
                {
                    yield return testCase;
                }
            }
        }

        static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue result;

            if (dictionary.TryGetValue(key, out result))
            {
                return result;
            }

            result = new TValue();
            dictionary[key] = result;

            return result;
        }
    }

    public class RepeatAttribute : DataAttribute
    {
        private readonly int _count;

        public RepeatAttribute(int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                      "Repeat count must be greater than 0.");
            }
            _count = count;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return Enumerable.Repeat(new object[0], _count);
        }
    }
}
