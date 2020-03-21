using System;

namespace OrToolsPickListSolver.RandomNumbers
{
    public class RandomKumaraswamy : Random
    {
        private readonly int _min;
        private readonly int _max;
        private readonly double _invA;
        private readonly double _invB;

        public RandomKumaraswamy(int min, int max, double a = 1.5, double b = 5)
        {
            _min = min;
            _max = max;
            _invA = 1.0 / a;
            _invB = 1.0 / b;
        }

        protected override double Sample()
        {
            return InverseCdf(base.Sample());
        }
    
        public override int Next()
        {
            var sample = Sample();
            return (int) (sample * (_max - _min)) + _min;
        }   

        private double InverseCdf(double u)
        {
            return Math.Pow(1 - Math.Pow(1 - u, _invB), _invA);
        }
    }

}
