// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public interface IRandomGenerator
    {
        void GenerateRandom(Byte[] buffer);
        void GenerateRandom(Byte[] buffer, Int32 offset, Int32 length);
    }
    public class RandomGenerator : IRandomGenerator
    {
        readonly Random random;
        public RandomGenerator(Random random)
        {
            this.random = random;
        }
        public void GenerateRandom(Byte[] bytes)
        {
            random.NextBytes(bytes);
        }
        public void GenerateRandom(Byte[] bytes, int offset, int length)
        {
            //buffer.EnsureCapacityCopyData(length);
            throw new NotImplementedException();
        }
    }
}
