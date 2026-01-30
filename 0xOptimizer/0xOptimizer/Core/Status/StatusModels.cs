using System;
using System.Collections.Generic;

namespace _0xOptimizer.Core.Status
{
    public sealed class StatusItem
    {
        public string Name { get; }
        public Func<bool> CheckApplied { get; }

        public StatusItem(string name, Func<bool> check)
        {
            Name = name;
            CheckApplied = check;
        }
    }

    public sealed class StatusCategory
    {
        public string Name { get; }
        public List<StatusItem> Items { get; } = new();
        public StatusCategory(string name) => Name = name;
    }
}