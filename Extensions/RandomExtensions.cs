using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Extensions
{
    public static class RandomExtensions
    {
        public static int RealNext(this Random r, int max) { return r.RealNext(0, max); }
        public static int RealNext(this Random r, int min, int max) { return r.Next(min, max + 1); } // Was that so hard, Microsoft? None of this "between 0 and max-1" crap. When I say 10 as max. I mean 10. Not 9. ಠ_ಠ
    }
}
