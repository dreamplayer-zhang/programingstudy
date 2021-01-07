using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public interface IRecipe : IComparable<IRecipe>
    {
        bool Save(string recipePath);
        bool Read(string recipePath);
    }
}
