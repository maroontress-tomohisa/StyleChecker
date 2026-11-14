namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    using System;

    public sealed class Okay
    {
        public void MethodReferences()
        {
            var all = new Action[]
            {
                Complex,
                Empty,
            };
        }

        public void MethodReferencesWithNull()
        {
            var all = new Action[]
            {
                Complex,
                null,
            };
        }

        public void Lambdas()
        {
            var all = new Func<int>[]
            {
                () => 1,
                () => 2,
            };
        }

        public void LambdasWithImplicitParameterTypes()
        {
            var all = new Func<int, int>[]
            {
                x => x,
                x => x + 1,
            };
        }

        public void AnonymousMethods()
        {
            var all = new Func<int>[]
            {
                delegate { return 1; },
                delegate { return 2; },
            };
        }

        public void Empty()
        {
        }

        public void Complex()
        {
        }
    }
}
