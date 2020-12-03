using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class TestClass : ObservableObject
    {
        private int a;
        private int b;
        private double c;
        private string d;

        public int A { get => a; set => SetProperty<int>(ref a, value); }
        public int B { get => b; set => SetProperty<int>(ref b, value); }
        public double C { get => c; set => SetProperty<double>(ref c, value); }
        public string D { get => d; set => SetProperty<string>(ref d, value); }
    }
}
