using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CashrewardsOffers.Application.AcceptanceTests.Helpers
{
    public static class TableExtensions
    {
        public static IEnumerable<T> CreateSetWithDefaults<T>(this Table table, IDictionary<string, object> defaults)
        {
            return table.CreateSet(row => CreateInstanceWithDefaults<T>(row, defaults));
        }

        public static IEnumerable<T> CreateDeepSet<T>(this Table table)
        {
            var set = table.CreateSet<T>().ToList();

            SetNestedClassTypes(set, table);

            return set;
        }

        public static IEnumerable<T> CreateDeepSetWithDefaults<T>(this Table table, IDictionary<string, object> defaults)
        {
            var set = table.CreateSet(row => CreateInstanceWithDefaults<T>(row, defaults)).ToList();

            SetNestedClassTypes(set, table);

            return set;
        }

        private static T CreateInstanceWithDefaults<T>(TableRow row, IDictionary<string, object> defaults)
        {
            var item = row.CreateInstance<T>();
            foreach (var keyValuePair in defaults)
            {
                if (!row.ContainsKey(keyValuePair.Key))
                {
                    var currentType = typeof(T);
                    PropertyInfo? currentProperty = null;
                    object? currentValue = null;
                    object? nextValue = item;
                    foreach (var propertyName in keyValuePair.Key.Split('.'))
                    {
                        currentValue = nextValue;
                        currentProperty = currentType.GetProperty(propertyName);
                        if (currentProperty == null)
                        {
                            throw new InvalidOperationException($"Property {keyValuePair.Key} was not found on type {typeof(T).Name}");
                        }

                        currentType = currentProperty.PropertyType;
                        nextValue = currentProperty.GetValue(currentValue);
                        if (nextValue == null)
                        {
                            nextValue = Activator.CreateInstance(currentType);
                            currentProperty.SetValue(currentValue, nextValue);
                        }
                    }

                    if (currentProperty == null)
                    {
                        throw new InvalidOperationException($"Property {keyValuePair.Key} was not found on type {typeof(T).Name}");
                    }

                    currentProperty.SetValue(currentValue, keyValuePair.Value);
                }
            }

            return item;
        }

        private static void SetNestedClassTypes<T>(List<T> set, Table table)
        {
            foreach (var propertyInfo in typeof(T).GetProperties().Where(p => p.PropertyType.IsClass))
            {
                var headers = table.Header.ToList();
                var subHeaders = new List<(int, string)>();
                for (int column = 0; column < headers.Count; column++)
                {
                    if (headers[column].StartsWith($"{propertyInfo.Name}."))
                    {
                        subHeaders.Add((column, headers[column].Substring(propertyInfo.Name.Length + 1)));
                    }
                }

                if (subHeaders.Count > 0)
                {
                    for (int row = 0; row < table.RowCount; row++)
                    {
                        var subtable = new Table(subHeaders.Select(h => h.Item2).ToArray());
                        List<string> subrow = new();
                        foreach (var subHeader in subHeaders)
                        {
                            subrow.Add(table.Rows[row][subHeader.Item1]);
                        }

                        subtable.AddRow(subrow.ToArray());
                        var propertyValue = propertyInfo.GetValue(set[row]);
                        if (propertyValue == null)
                        {
                            propertyValue = Activator.CreateInstance(propertyInfo.PropertyType);
                            propertyInfo.SetValue(set[row], propertyValue);
                        }

                        TableHelperExtensionMethods.FillInstance(subtable, propertyValue);
                    }
                }
            }
        }
    }
}
