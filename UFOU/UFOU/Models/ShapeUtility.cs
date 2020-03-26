using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UFOU.Models
{
    /// <summary>
    /// Small utility class for defining shape categories
    /// </summary>
    public static class ShapeUtility
    {
        private static Dictionary<string, Shape> _aliases = new Dictionary<string, Shape>()
        {
            { "crescent", Shape.Circle },
            { "sphere", Shape.Circle },
            { "dome", Shape.Circle },
            { "cone", Shape.Triangle },
            { "pyramid", Shape.Triangle },
            { "triangular", Shape.Triangle },
            { "delta", Shape.Chevron },
            { "flare", Shape.Fireball },
            { "round", Shape.Oval }
        };

        /// <summary>
        /// Set of shapes defined by NUFORC in their database
        /// </summary>
        public enum Shape
        {
            Circle, Rectangle, Triangle,
            Chevron, Cigar, Cross,
            Cylinder, Diamond, Disk,
            Egg, Fireball, Flash,
            Formation, Hexagon, Light,
            Oval, Teardrop, Changing,
            Other, Unknown, Unspecified
        }

        /// <summary>
        /// Returns the shape aliased by the given string
        ///     i.e. "sphere" maps to Shape.Circle
        /// Returns Shape.Other if no alias can be found
        /// </summary>
        /// <param name="shapeStr">string holding the name of a shape, case insensitive</param>
        public static Shape ShapeAliases(string shapeStr)
        {
            if (_aliases.TryGetValue(shapeStr.ToLower(), out Shape shape))
                return shape;
            else
                return Shape.Other;
        }

        /// <summary>
        /// Returns a copy of the shape aliases by this class
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Shape> GetAllAliases()
        {
            return new Dictionary<string, Shape>(_aliases);
        }
    }
}
