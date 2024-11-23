using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NF.Tool.PatchNoteMaker.Common.Fragments
{
    public sealed class Fragment : IEnumerable<KeyValuePair<string, Section>>
    {
        Dictionary<string, Section> _dic = new Dictionary<string, Section>();

        public void AddSection([NotNull] Section section)
        {
            _dic[section.Name] = section;
        }

        public bool ContainsKey(string sectionName)
        {
            return _dic.ContainsKey(sectionName);
        }

        public IEnumerator<KeyValuePair<string, Section>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }
    }
}
