﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SymbolicAlgebra
{
    public partial class SymbolicVariable : ICloneable
    {

        public static SymbolicVariable operator /(SymbolicVariable a, SymbolicVariable b)
        {

            SymbolicVariable sv = (SymbolicVariable)a.Clone();

            // if the divided term is more than on term
            // x^2/(y-x)  ==>  
            if (b.AddedTerms.Count > 0)
            {

                //multiply divided term by this value
                sv.DividedTerm = sv.DividedTerm * b;

                return sv;
            }


            SymbolicVariable subB = (SymbolicVariable)b.Clone();

            SymbolicVariable total = default(SymbolicVariable);

            int subIndex = 0;

            subB._AddedTerms = null;   // remove added variables to prevent its repeated calculations in second passes
            // or to make sure nothing bad happens {my idiot design :S)

            if (a.SymbolsEquals(subB))
            {
                sv.Coeffecient = sv.Coeffecient / subB.Coeffecient;
                sv.SymbolPower = sv.SymbolPower - subB.SymbolPower;

                //fuse the fused symbols in b into sv
                foreach (var bfv in subB.FusedSymbols)
                {
                    if (sv.FusedSymbols.ContainsKey(bfv.Key))
                        sv.FusedSymbols[bfv.Key] -= bfv.Value;
                    else
                        sv.FusedSymbols.Add(bfv.Key, -1 * bfv.Value);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(sv.Symbol))
                {
                    // the instance have an empty primary variable so we should add it 
                    sv.Symbol = subB.Symbol;
                    sv.SymbolPower = -1 * subB.SymbolPower;

                    //fuse the fusedvariables in b into sv
                    foreach (var bfv in subB.FusedSymbols)
                    {
                        if (sv.FusedSymbols.ContainsKey(bfv.Key))
                            sv.FusedSymbols[bfv.Key] -= bfv.Value;
                        else
                            sv.FusedSymbols.Add(bfv.Key, -1 * bfv.Value);
                    }

                }
                else
                {
                    if (sv.Symbol.Equals(subB.Symbol, StringComparison.OrdinalIgnoreCase))
                    {
                        sv.SymbolPower -= subB.SymbolPower;
                    }
                    else if (sv.FusedSymbols.ContainsKey(subB.Symbol))
                    {
                        sv.FusedSymbols[subB.Symbol] -= subB.SymbolPower;
                    }
                    else
                    {
                        sv.FusedSymbols.Add(subB.Symbol, -1 * subB.SymbolPower);
                    }
                }

                sv.Coeffecient = a.Coeffecient / subB.Coeffecient;

            }

            if (sv.AddedTerms.Count > 0)
            {
                Dictionary<string, SymbolicVariable> newAddedVariables = new Dictionary<string, SymbolicVariable>(StringComparer.OrdinalIgnoreCase);
                foreach (var vv in sv.AddedTerms)
                {
                    var newv = vv.Value / subB;
                    newAddedVariables.Add(newv.SymbolBaseValue, newv);

                }
                sv._AddedTerms = newAddedVariables;
            }

        np:
            if (subIndex < b.AddedTerms.Count)
            {
                // we should multiply other parts also 
                // then add it to the current instance

                // there are still terms to be consumed 
                //   this new term is a sub term in b and will be added to all terms of a.
                subB = b.AddedTerms.ElementAt(subIndex).Value;

                if (total != null) total = total + (a / subB);
                else total = sv + (a / subB);

                subIndex = subIndex + 1;  //increase 
                goto np;
            }
            else
            {
                if (total == null) total = sv;
            }


            AdjustZeroPowerTerms(total);
            AdjustZeroCoeffecientTerms(total);

            return total; //RemoveZeroTerms(total);
        }

    }
}