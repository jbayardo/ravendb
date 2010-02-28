using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rhino.DivanDB.Json;

namespace Rhino.DivanDB.Linq
{
    public delegate IEnumerable IndexingFunc(IEnumerable<JsonDynamicObject> source);

    public class AbstractViewGenerator
    {
        private IndexingFunc compiledDefinition;
        public string ViewText { get; set; }

        public Expression<IndexingFunc> IndexDefinition { get; protected set; }

        public AbstractViewGenerator()
        {
            AccessedFields = new HashSet<string>();
        }

        public Type GeneratedType
        {
            get
            {
                return IndexDefinition.Body.Type.GetGenericArguments()[0];
            }
        }

        public IEnumerable Execute(IEnumerable<JsonDynamicObject> source)
        {
            ForceCompilationIfNeeded();
            return compiledDefinition(source);
        }

        public IndexingFunc CompiledDefinition
        {
            get
            {
                ForceCompilationIfNeeded();
                return compiledDefinition;
            }
        }

        public HashSet<string> AccessedFields { get; set; }

        private void ForceCompilationIfNeeded()
        {
            if (compiledDefinition == null)
            {
                compiledDefinition = IndexDefinition.Compile();
            }
        }
    }
}