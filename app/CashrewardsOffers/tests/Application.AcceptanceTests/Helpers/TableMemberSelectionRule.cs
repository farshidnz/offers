using FluentAssertions.Equivalency;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace CashrewardsOffers.Application.AcceptanceTests.Helpers
{
    // FluentAssertions member selection rule
    // TableMemberSelectionRule selects only properties specified in columns in a specflow Table object
    // Nested class members can be included.
    // e.g. include a table column like | Merchant.Id |
    public class TableMemberSelectionRule : IMemberSelectionRule
    {
        private readonly HashSet<string> _includedMembers = new();

        public TableMemberSelectionRule(Table table)
        {
            foreach (var header in table.Header)
            {
                _includedMembers.Add(header);

                int dotpos = header.IndexOf('.');
                while (dotpos != -1)
                {
                    _includedMembers.Add(header.Substring(0, dotpos));
                    dotpos = header.IndexOf('.', dotpos + 1);
                }
            }
        }

        public bool IncludesMembers => false;

        public IEnumerable<IMember> SelectMembers(INode currentNode, IEnumerable<IMember> selectedMembers, MemberSelectionContext context) =>
            selectedMembers.Where(m => _includedMembers.Contains(RemoveLeadingArray(m.PathAndName)));

        private static string RemoveLeadingArray(string path)
        {
            var brace = path.IndexOf("].");
            if (brace == -1) return path;
            return path.Substring(brace + 2, path.Length - brace - 2);
        }
    }
}
