// SPDX-License-Identifier: GPL-2.0
// Compute thermodynamic properties of individual species and composition of species
// 
// Original C: Copyright (C) 2000
//    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>
//    Mark Pinese  <pinese@cyberwizards.com.au>
//
// C# Port: Copyright (C) 2022
//    Ben Voß
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

/* equilibrium.c  -  Responsible of the chemical equilibrium          */
/* $Id: equilibrium.c,v 1.1 2000/07/14 00:30:53 antoine Exp $ */
/* Copyright (C) 2000                                                  */
/*    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>                   */
/*    Mark Pinese <pinese@cyberwizards.com.au>                         */
/*                                                                     */
/* Licensed under the GPLv2                                            */

using ImpulseRocketry.LibNum;

namespace ImpulseRocketry.LibPropellantEval;

public partial class Evaluator {

    // Initial temperature estimate for problem with not-fixed temperature
    private const int ESTIMATED_T = 3800;

    private const double CONC_TOL = 1.0e-8;
    private const double LOG_CONC_TOL = -18.420681;
    private const double CONV_TOL = 0.5e-5;

    private const int ITERATION_MAX = 100;

    /// <summary>
    /// This function searches for all elements present in the composition and fill the list with the corresponding number.
    /// 
    /// COMMENTS: It fill the member element in <see cref="LibPropellantEval.Equilibrium"/>
    /// 
    /// DATE: February 6, 2000
    /// 
    /// AUTHOR: Antoine Lefebvre
    /// </summary>
    /// <param name="e">Holds the information about the propellant composition.</param>
    public int ListElement(Equilibrium e) {
        int n = 0;

        var prop = e.Propellant;
        var prod = e.Product;

        // Reset the lement vector to -1
        e.ResetElementList();

        for (var i = 0; i < e.Propellant.NumComponents; i++) {
            // maximum of 6 different atoms in the composition
            for (var j = 0; j < 6; j++) {
                if (!(PropellantList[prop.Molecule[i]].Coef[j] == 0)) {
                    // get the element
                    var t = PropellantList[prop.Molecule[i]].Elem[j];

                    for (var k = 0; k <= n; k++) {
                        // verify if the element was not already in the list
                        if (prod.Elements[k] == t) {
                            break;
                        }

                        // if we have checked each element, add it to the list
                        if (k == n) {
                            if (n == Constants.MAX_ELEMENT) {
                                Console.Error.WriteLine("Maximum of {MAX_ELEMENT} elements. Abort.");
                            }

                            prod.Elements[n] = t;
                            n++;
                            break;
                        }
                    }
                }
            }
        }

        prod.NumElements = n;
        prod.ElementListed = true;

        return n;
    }

    /// <summary>
    /// This function search in thermo_list for all molecule that could be form with one or more of the element
    /// in element_list. The function fill product_list with the corresponding number of these molecule.
    ///
    /// DATE: February 6, 2000
    /// 
    /// AUTHOR: Antoine Lefebvre
    /// </summary>
    /// <param name="e">The equilibrium_t class</param>
    /// <returns>The number of elements found</returns>
    public int ListProduct(Equilibrium e) {
        int n = 0;   // global counter (number of species found)
        int st;      // temporary variable to hold the state of one specie
        var ok = true;

        var prod = e.Product;

        // reset the product to zero
        prod.NumSpecies[Constants.GAS] = 0;
        prod.NumSpecies[Constants.CONDENSED] = 0;

        for (var j = 0; j < ThermoList.Count; j++) {
            // for each of the five possible element of a species
            for (var k = 0; k < 5; k++) {
                if (!(ThermoList[j].Coef[k] == 0)) {
                    for (var i = 0; i < prod.NumElements; i++) {
                        if (prod.Elements[i] == ThermoList[j].Elem[k]) {
                            break;
                        } else if (i == (prod.NumElements - 1)) {
                            ok = false;
                        }
                    }

                    if (!ok) {
                        break;
                    }
                }
            }

            if (ok) { // add to the list 
                st = ThermoList[j].State;

                prod.Species[st][prod.NumSpecies[st]] = (short)j;
                prod.NumSpecies[st]++;
                n++;

                if ((prod.NumSpecies[Constants.GAS] > Constants.MAX_PRODUCT) || (prod.NumSpecies[Constants.CONDENSED] > Constants.MAX_PRODUCT)) {
                    Console.Error.WriteLine($"Error: Maximum of {Constants.MAX_PRODUCT} differents product reached.");
                    Console.Error.WriteLine("       Change MAX_PRODUCT and recompile!\n");
                    return -1;
                }

            }
            ok = true;
        }

        prod.NumCondensed = prod.NumSpecies[Constants.CONDENSED];

        /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          move it to the equilibrium function
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        // initialize tho mol number to 0.1mol/(nb of gazeous species)
        e.IterationInfo.N = e.IterationInfo.SumN = 0.1;
        e.IterationInfo.LnN = Math.Log(e.IterationInfo.N);

        for (var i = 0; i < e.Product.NumSpecies[Constants.GAS]; i++) {
            e.Product.Coef[Constants.GAS][i] = 0.1 / e.Product.NumSpecies[Constants.GAS];
            e.IterationInfo.LnNj[i] = Math.Log(e.Product.Coef[Constants.GAS][i]);
        }

        // initialize CONDENSED to zero
        for (var i = 0; i < e.Product.NumSpecies[Constants.CONDENSED]; i++) {
            e.Product.Coef[Constants.CONDENSED][i] = 0;
        }

        e.Product.ProductListed = true;

        return n;
    }

    // Mass of propellant in gram
    private double PropellantMass(Equilibrium e) {
        double mass = 0.0;
        for (var i = 0; i < e.Propellant.NumComponents; i++) {
            mass += e.Propellant.Coef[i] * PropellantList.MolarMass(e.Propellant.Molecule[i]);
        }
        return mass;
    }

    private void ComputeThermoProperties(Equilibrium e) {
        var pr = e.Properties;

        // Compute equilibrium properties
        pr.H = ProductEnthalpy(e) * Constants.R * pr.T;
        pr.U = (ProductEnthalpy(e) - e.IterationInfo.N) * Constants.R * pr.T;
        pr.G = (ProductEnthalpy(e) - ProductEntropy(e)) * Constants.R * pr.T;
        pr.S = ProductEntropy(e) * Constants.R;
        pr.M = e.ProductMolarMass;
    }

    private void FillEquilibriumMatrix(double[][] matrix, Equilibrium e, ProblemType P) {
        double tmp, mol;

        // position of the right side dependeing on the type of problem
        short roff = 2;

        double[][] Mu = Utils.Make2DArray<double>(Constants.STATE_LAST, Constants.MAX_PRODUCT); // gibbs free energy for GASes
        double[][] Ho = Utils.Make2DArray<double>(Constants.STATE_LAST, Constants.MAX_PRODUCT); // enthalpy in the standard state

        /* The matrix is separated in five parts
          1- lagrangian multiplier (start at zero)
          2- delta(nj) for CONDENSED (start at n_element)
          3- delta(ln n) (start at n_element + n[Constants.CONDENSED])
          4- delta(ln T) (start at n_element + n[Constants.CONDENSED] + 1)
          5- right side (start at n_element + n[Constants.CONDENSED] + roff)

          we defined one index for 2, 3 and 4
          the first start to zero and the last is the matrix size
        */
        var p = e.Product;
        var pr = e.Properties;
        var it = e.IterationInfo;

        if (P == ProblemType.TP) {
            roff = 1;
        }

        var idx_cond = p.NumElements;
        var idx_n = p.NumElements + p.NumSpecies[Constants.CONDENSED];
        var idx_T = p.NumElements + p.NumSpecies[Constants.CONDENSED] + 1;

        var size = p.NumElements + p.NumSpecies[Constants.CONDENSED] + roff;

        mol = it.SumN;

        for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
            Mu[Constants.GAS][k] = ThermoList.Gibbs(p.Species[Constants.GAS][k], Constants.GAS, it.LnNj[k] - it.LnN, (float)pr.T, (float)pr.P);
            Ho[Constants.GAS][k] = ThermoList.Enthalpy0(p.Species[Constants.GAS][k], (float)pr.T);
        }

        for (var k = 0; k < p.NumSpecies[Constants.CONDENSED]; k++) {
            Mu[Constants.CONDENSED][k] = ThermoList.Gibbs(p.Species[Constants.CONDENSED][k], Constants.CONDENSED, 0, (float)pr.T, (float)pr.P);
            Ho[Constants.CONDENSED][k] = ThermoList.Enthalpy0(p.Species[Constants.CONDENSED][k], (float)pr.T);
        }

        // fill the common part of the matrix
        FillMatrix(matrix, e);

        // delta ln(T) (for SP and HP only)
        if (P != ProblemType.TP) {
            for (var j = 0; j < p.NumElements; j++) {
                tmp = 0.0;
                for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                    tmp += p.A[j][k] * p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k];
                }
                matrix[j][idx_T] = tmp;
            }
        }

        // right side
        for (var j = 0; j < p.NumElements; j++) {
            tmp = 0.0;

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++)
                tmp += p.A[j][k] * p.Coef[Constants.GAS][k] * Mu[Constants.GAS][k];

            // b[i]
            for (var k = 0; k < Constants.STATE_LAST; k++)
                for (var i = 0; i < p.NumSpecies[k]; i++)
                    tmp -= ThermoList.ProductElementCoef(p.Elements[j], p.Species[k][i]) *
                      p.Coef[k][i];

            // b[i]o
            // 04/06/2000 - division by propellant_mass(e)
            for (var i = 0; i < e.Propellant.NumComponents; i++)
                tmp += PropellantList.PropellantElementCoef(p.Elements[j], e.Propellant.Molecule[i]) *
                  e.Propellant.Coef[i] / PropellantMass(e);

            matrix[j][size] = tmp;
        }

        // delta ln(T)
        if (P != ProblemType.TP) {
            for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) { // row
                matrix[j + idx_cond][idx_T] = Ho[Constants.CONDENSED][j];
            }
        }

        // right side
        for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) // row
        {
            matrix[j + idx_cond][size] = Mu[Constants.CONDENSED][j];
        }

        // delta ln(n)
        matrix[idx_n][idx_n] = mol - it.N;

        // delta ln(T)
        if (P != ProblemType.TP) {
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++)
                tmp += p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k];
            matrix[idx_n][idx_T] = tmp;
        }

        // right side
        tmp = 0.0;
        for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
            tmp += p.Coef[Constants.GAS][k] * Mu[Constants.GAS][k];
        }

        matrix[idx_n][size] = it.N - mol + tmp;

        // for enthalpy/pressure problem 
        if (P == ProblemType.HP) {
            // part with lagrangian multipliers
            for (var i = 0; i < p.NumElements; i++) // each column
            {
                tmp = 0.0;
                for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++)
                    tmp += p.A[i][k] * p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k];

                matrix[idx_T][i] = tmp;
            }

            // Delta n
            for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++)
                matrix[idx_T][i + idx_cond] = Ho[Constants.CONDENSED][i];

            // Delta ln(n)
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k];
            }

            matrix[idx_T][idx_n] = tmp;

            // Delta ln(T)
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * ThermoList.SpecificHeat0(p.Species[Constants.GAS][k], (float)pr.T);
            }

            for (var k = 0; k < p.NumSpecies[Constants.CONDENSED]; k++) {
                tmp += p.Coef[Constants.CONDENSED][k] * ThermoList.SpecificHeat0(p.Species[Constants.CONDENSED][k], (float)pr.T);
            }

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k] * Ho[Constants.GAS][k];
            }

            matrix[idx_T][idx_T] = tmp;

            // right side
            tmp = PropellantEnthalpy(e) / (Constants.R * pr.T) - ProductEnthalpy(e);

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k] * Mu[Constants.GAS][k];
            }

            matrix[idx_T][size] = tmp;
        } else if (P == ProblemType.SP) {
            // for entropy/pressure problem
            // part with lagrangian multipliers
            for (var i = 0; i < p.NumElements; i++) {
                tmp = 0.0;
                for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                    tmp += p.A[i][k] * p.Coef[Constants.GAS][k] *
                      ThermoList.Entropy(p.Species[Constants.GAS][k], Constants.GAS, it.LnNj[k] - it.LnN, (float)pr.T, (float)pr.P);
                }

                matrix[idx_T][i] = tmp;
            }

            // Delta n
            for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
                matrix[idx_T][i + idx_cond] =
                  ThermoList.Entropy0(p.Species[Constants.CONDENSED][i], (float)pr.T); // ok for CONDENSED
            }

            // Delta ln(n)
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * ThermoList.Entropy(p.Species[Constants.GAS][k], Constants.GAS, it.LnNj[k]
                                                - it.LnN, (float)pr.T, (float)pr.P);
            }

            matrix[idx_T][idx_n] = tmp;

            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * ThermoList.SpecificHeat0(p.Species[Constants.GAS][k], (float)pr.T);
            }

            for (var k = 0; k < p.NumSpecies[Constants.CONDENSED]; k++) {
                tmp += p.Coef[Constants.CONDENSED][k] *
                  ThermoList.SpecificHeat0(p.Species[Constants.CONDENSED][k], (float)pr.T);
            }

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k] * Ho[Constants.GAS][k] *
                  ThermoList.Entropy(p.Species[Constants.GAS][k], Constants.GAS, it.LnNj[k] - it.LnN, (float)pr.T, (float)pr.P);
            }

            matrix[idx_T][idx_T] = tmp;

            // entropy of reactant
            tmp = e.Entropy; // assign entropy
            tmp -= ProductEntropy(e);
            tmp += it.N;

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp -= p.Coef[Constants.GAS][k];
            }

            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.Coef[Constants.GAS][k]
                  * Mu[Constants.GAS][k]
                  * ThermoList.Entropy(p.Species[Constants.GAS][k], Constants.GAS, it.LnNj[k] - it.LnN,
                            (float)pr.T, (float)pr.P);
            }

            matrix[idx_T][size] = tmp;
        }

    }

    /// <summary>
    /// Fill the matrix in function of the data store in the structure equilibrium_t. The solution
    /// of this matrix give corresction to initial estimate.
    ///
    /// AUTHOR:   Antoine Lefebvre
    /// </summary>
    /// <remarks>
    /// It use the theory explained in  "Computer Program for Calculation of Complex Chemical
    /// Equilibrium Compositions, Rocket Performance, Incident and Reflected Shocks, and Chapman-Jouguet
    /// Detonations" by Gordon and McBride
    /// </remarks>
    // This part of the matrix is the same for equilibrium and derivative
    private void FillMatrix(double[][] matrix, Equilibrium e) {
        var p = e.Product;
        var idx_cond = p.NumElements;
        var idx_n = p.NumElements + p.NumSpecies[Constants.CONDENSED];

        // Fill the matrix (part with the Lagrange multipliers)
        for (var i = 0; i < p.NumElements; i++) { // each column 
            for (var j = 0; j < p.NumElements; j++) { // each row 
                var tmp = 0.0;
                for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                    tmp += p.A[j][k] * p.A[i][k] * p.Coef[Constants.GAS][k];
                }
                matrix[j][i] = tmp;
            }
        }

        // Delta n
        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) { // column
            for (var j = 0; j < p.NumElements; j++) { // row
                matrix[j][i + idx_cond] = ThermoList.ProductElementCoef(p.Elements[j], p.Species[Constants.CONDENSED][i]);
            }
        }

        // Delta ln(n)
        for (var j = 0; j < p.NumElements; j++) {
            var tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.A[j][k] * p.Coef[Constants.GAS][k];
            }
            matrix[j][idx_n] = tmp;
        }

        // Second row
        for (var i = 0; i < p.NumElements; i++) { // column
            for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) { // row 
                                                       // copy the symetric part of the matrix
                matrix[j + idx_cond][i] = matrix[i][j + idx_cond];
            }
        }

        // Set to zero
        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED] + 1; i++) { // column
            for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) { // row
                matrix[j + idx_cond][i + idx_cond] = 0.0;
            }
        }

        // Third row
        for (var i = 0; i < p.NumElements; i++) { // each column
                                                // copy the symetric part of the matrix
            matrix[idx_n][i] = matrix[i][idx_n];
        }

        // Set to zero
        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) { // column
            matrix[idx_n][i + idx_cond] = 0.0;
        }

        if (_verbose > 0) {
            MatrixUtils.PrintMatrix(matrix, p.NumElements);
        }
    }

    private bool RemoveCondensed(ref int n, Equilibrium e) {
        bool r = false;
        bool ok = true;

        var p = e.Product;
        var pr = e.Properties;

        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
            // if a condensed have negative coefficient, we should remove it
            if (p.Coef[Constants.CONDENSED][i] <= 0.0) {
                if (_verbose > 1) {
                    Console.WriteLine($"{ThermoList[p.Species[Constants.CONDENSED][i]].Name} should be remove, negative concentration.");
                }

                // Remove from the list ( put it at the end for later use )
                var pos = p.Species[Constants.CONDENSED][i];

                for (var j = i; j < p.NumSpecies[Constants.CONDENSED] - 1; j++) {
                    p.Species[Constants.CONDENSED][j] = p.Species[Constants.CONDENSED][j + 1];
                }
                p.Species[Constants.CONDENSED][p.NumSpecies[Constants.CONDENSED] - 1] = pos;

                p.NumSpecies[Constants.CONDENSED]--;

                r = true;
            } else if (!ThermoList.TemperatureCheck(p.Species[Constants.CONDENSED][i], (float)pr.T)) {
                /* if the condensed species is present outside of the temperature
                  range at which it could exist, we should either replace it by
                  an other phase or add the other phase. If the difference between
                  the melting point and the temperature is over 50 k, we replace,
                  else we add the other phase. */

                // Find the new molecule
                for (var j = p.NumSpecies[Constants.CONDENSED]; j < n; j++) {
                    // if this is the same molecule and temperature_check is true,
                    // than it is the good molecule

                    for (var k = 0; k < 5; k++) {
                        if (!(
                            (ThermoList[p.Species[Constants.CONDENSED][i]].Coef[k] == ThermoList[p.Species[Constants.CONDENSED][j]].Coef[k]) &&
                            (ThermoList[p.Species[Constants.CONDENSED][i]].Elem[k] == ThermoList[p.Species[Constants.CONDENSED][j]].Elem[k]) &&
                            (p.Species[Constants.CONDENSED][i] != p.Species[Constants.CONDENSED][j])))
                        {
                            ok = false;
                        }
                    }

                    // Replace or add the molecule
                    if (ok) {
                        if (Math.Abs(pr.T - ThermoList.TransitionTemperature(p.Species[Constants.CONDENSED][j], (float)pr.T)) > 50.0) {
                            // Replace the molecule
                            if (_verbose > 1) {
                                Console.WriteLine($"{ThermoList[p.Species[Constants.CONDENSED][i]].Name} should be replace by {ThermoList[p.Species[Constants.CONDENSED][j]].Name}");
                            }

                            var pos = p.Species[Constants.CONDENSED][i];
                            p.Species[Constants.CONDENSED][i] = p.Species[Constants.CONDENSED][j];
                            p.Species[Constants.CONDENSED][j] = pos;

                        } else {
                            // Add the molecule
                            if (_verbose > 1) {
                                Console.WriteLine($"{ThermoList[p.Species[Constants.CONDENSED][i]].Name} should be add with {ThermoList[p.Species[Constants.CONDENSED][j]].Name}");
                            }

                            // To include the species, exchange the value
                            var pos = p.Species[Constants.CONDENSED][p.NumSpecies[Constants.CONDENSED]];
                            p.Species[Constants.CONDENSED][p.NumSpecies[Constants.CONDENSED]] =
                              p.Species[Constants.CONDENSED][i];
                            p.Species[Constants.CONDENSED][i] = pos;

                            p.NumSpecies[Constants.CONDENSED]++;
                        }

                        r = true; // A species have been replace

                        // we do not need to continue searching so we break
                        break;
                    }

                    ok = true;
                }
            }
        } // for each condensed

        // false if none removed
        return r;
    }

    private bool IncludeCondensed(ref int n, Equilibrium e, double[] sol) {
        var p = e.Product;
        var pr = e.Properties;

        var tmp = 0.0;
        var j = -1;

        // We include a condensed if it minimize the gibbs free energy and if it could exist at the chamber temperature
        for (var i = p.NumSpecies[Constants.CONDENSED]; i < n; i++) {
            if (ThermoList.TemperatureCheck(p.Species[Constants.CONDENSED][i], (float)pr.T)) {
                var temp = 0.0;
                for (var k = 0; k < p.NumElements; k++) {
                    temp += sol[k] * ThermoList.ProductElementCoef(p.Elements[k],
                                                        p.Species[Constants.CONDENSED][i]);
                }

                if (ThermoList.Gibbs0(p.Species[Constants.CONDENSED][i], (float)pr.T) - temp < tmp) {
                    tmp = ThermoList.Gibbs0(p.Species[Constants.CONDENSED][i], (float)pr.T) - temp;
                    j = i;
                }
            }
        }

        // In the case we found a species that minimize the gibbs energy, we should include it
        if (j != -1) {

            if (_verbose > 1) {
                Console.WriteLine($"{ThermoList[e.Product.Species[Constants.CONDENSED][j]].Name} should be include");
            }

            // to include the species, exchange the value
            var pos = p.Species[Constants.CONDENSED][p.NumSpecies[Constants.CONDENSED]];
            p.Species[Constants.CONDENSED][p.NumSpecies[Constants.CONDENSED]] =
              p.Species[Constants.CONDENSED][j];
            p.Species[Constants.CONDENSED][j] = pos;

            p.NumSpecies[Constants.CONDENSED]++;

            return true;
        }

        return false;
    }

    private void NewApproximation(Equilibrium e, double[] sol, ProblemType P) {
        int i, j;

        // control factor
        double lambda1, lambda2, lambda;

        double temp;

        var p = e.Product;
        var pr = e.Properties;
        var it = e.IterationInfo;

        // compute the values of delta ln(nj)
        it.DeltaLnN = sol[p.NumElements + p.NumSpecies[Constants.CONDENSED]];

        if (P != ProblemType.TP) {
            it.DeltaLnT = sol[p.NumElements + p.NumSpecies[Constants.CONDENSED] + 1];
        } else {
            it.DeltaLnT = 0.0;
        }

        for (i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            temp = 0.0;
            for (j = 0; j < p.NumElements; j++) {
                temp += p.A[j][i] * sol[j];
            }

            it.DeltaLnNj[i] =
              -ThermoList.Gibbs(p.Species[Constants.GAS][i], Constants.GAS, it.LnNj[i] - it.LnN, (float)pr.T, (float)pr.P)
              + temp + it.DeltaLnN
              + ThermoList.Enthalpy0(p.Species[Constants.GAS][i], (float)pr.T) * it.DeltaLnT;
        }

        lambda2 = 1.0;
        lambda1 = Math.Max(Math.Abs(it.DeltaLnT), Math.Abs(it.DeltaLnN));
        lambda1 = 5 * lambda1;
        for (i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            if (it.DeltaLnNj[i] > 0.0) {
                if (it.LnNj[i] - it.LnN <= LOG_CONC_TOL) {
                    lambda2 = Math.Min(lambda2,
                                    Math.Abs(((-it.LnNj[i] + it.LnN - 9.2103404)
                                          / (it.DeltaLnNj[i] - it.DeltaLnN))));
                } else if (it.DeltaLnNj[i] > lambda1) {
                    lambda1 = it.DeltaLnNj[i];
                }
            }
        }

        lambda1 = 2.0 / lambda1;

        lambda = Utils.Min(1.0, lambda1, lambda2);

        if (_verbose > 2) {
            Console.WriteLine($"lambda = {lambda:0.0000000000}, lambda1 = {lambda1:0.0000000000}, lambda2 = {lambda2:0.0000000000}");
            Console.WriteLine(" \t  nj \t\t  ln_nj_n \t Delta ln(nj)");

            for (i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
                Console.WriteLine($"{ThermoList[p.Species[Constants.GAS][i]].Name} \t {p.Coef[Constants.GAS][i]: 0.0000e+00;-0.0000e+00} \t {it.LnNj[i]: 0.0000e+00;-0.0000e+00} \t {it.DeltaLnNj[i]: 0.0000e+00;-0.0000e+00}");
            }
        }

        it.SumN = 0.0;

        // compute the new value for nj (gazeous) and ln_nj
        for (i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            it.LnNj[i] = it.LnNj[i] + lambda * it.DeltaLnNj[i];

            if (it.LnNj[i] - it.LnN <= LOG_CONC_TOL) {
                p.Coef[Constants.GAS][i] = 0.0;
            } else {
                p.Coef[Constants.GAS][i] = Math.Exp(it.LnNj[i]);
                it.SumN += p.Coef[Constants.GAS][i];
            }
        }

        // compute the new value for nj (CONDENSED)
        for (i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
            p.Coef[Constants.CONDENSED][i] = p.Coef[Constants.CONDENSED][i] +
              lambda * sol[p.NumElements + i];
        }

        if (_verbose > 2) {
            for (i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
                Console.WriteLine($"{ThermoList[p.Species[Constants.CONDENSED][i]].Name}: \t {p.Coef[Constants.CONDENSED][i]}");
            }
        }

        // new value of T
        if (P != ProblemType.TP) {
            pr.T = Math.Exp(Math.Log(pr.T) + lambda * it.DeltaLnT);
        }

        if (_verbose > 2) {
            Console.WriteLine($"Temperature: {pr.T:0.000000}");
        }

        // new value of n
        it.LnN += lambda * it.DeltaLnN;
        it.N = Math.Exp(it.LnN);
    }

    private static bool Convergence(Equilibrium e, double[] sol) {
        // For convergence test, mol is the sum of all mol even condensed
        var mol = e.IterationInfo.SumN;

        // Check for convergence
        for (var i = 0; i < e.Product.NumSpecies[Constants.GAS]; i++) {
            if (!(e.Product.Coef[Constants.GAS][i] * Math.Abs(e.IterationInfo.DeltaLnNj[i]) / mol <= CONV_TOL)) {
                return false; // haven't converge yet
            }
        }

        for (var i = 0; i < e.Product.NumSpecies[Constants.CONDENSED]; i++) {
            // test for the condensed phase
            if (!(sol[e.Product.NumElements + 1] / mol <= CONV_TOL)) {
                return false; // haven't converge yet
            }
        }

        if (!(e.IterationInfo.N * Math.Abs(e.IterationInfo.DeltaLnN) / mol <= CONV_TOL)) {
            return false; // haven't converge yet
        }

        if (!(Math.Abs(e.IterationInfo.DeltaLnT) <= 1.0e-4)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Compute the equilibrium composition at at specific pressure/temperature point. It uses FillMatrix
    /// to obtain correction to initial estimate. It correct the value until equilibrium is obtained.
    ///
    /// AUTHOR:   Antoine Lefebvre
    /// </summary>
    public bool Equilibrium(Equilibrium equil, ProblemType P) {
        bool stop = false;
        bool gas_reinserted = false;

        var p = equil.Product;

        // Position of the right side of the matrix dependeing on the type of problem
        int roff = 2;

        if (P == ProblemType.TP) {
            roff = 1;
        }

        // Initial temperature for assign enthalpy, entropy/pressure
        if (P != ProblemType.TP) {
            equil.Properties.T = ESTIMATED_T;
        }

        // If the element and the product haven't been listed
        if (!equil.Product.ElementListed) {
            ListElement(equil);
        }

        if (!equil.Product.ProductListed) {
            if (ListProduct(equil) == -1) {
                return false;
            }
        }

        // Build up the coefficient matrix
        for (var i = 0; i < p.NumElements; i++) {
            for (var j = 0; j < p.NumSpecies[Constants.GAS]; j++) {
                p.A[i][j] = ThermoList.ProductElementCoef(p.Elements[i], p.Species[Constants.GAS][j]);
            }
        }

        // For the first equilibrium, we do not consider the condensed species.
        if (!equil.Product.IsEquilibrium) {
            equil.Product.NumSpecies[Constants.CONDENSED] = 0;
            equil.IterationInfo.N = 0.1; // initial estimate of the mol number
        }

        // The size of the coefficient matrix
        var size = equil.Product.NumElements + equil.Product.NumSpecies[Constants.CONDENSED] + roff;

        // Allocate the memory for the matrix
        var matrix = Utils.Make2DArray<double>(size, size + 1);

        // Allocate the memory for the solution vector
        var sol = new double[size];

        // Main loop
        int k;
        for (k = 0; k < ITERATION_MAX; k++) {
            // Initially we haven't a good solution
            var solution_ok = false;

            while (!solution_ok) {
                FillEquilibriumMatrix(matrix, equil, P);

                if (_verbose > 2) {
                    Console.WriteLine($"Iteration {k + 1}");
                    MatrixUtils.PrintMatrix(matrix, size);
                }

                // solve the matrix
                if (!MatrixUtils.Lu(matrix, sol, size)) {
                    // the matrix have no unique solution
                    Console.WriteLine("The matrix is singular, removing excess condensed.");

                    // Try removing excess condensed
                    if (!RemoveCondensed(ref equil.Product.NumCondensed, equil)) {
                        if (gas_reinserted) {
                            Console.WriteLine("ERROR: No convergence, don't trust results");
                            // finish the main loop
                            stop = true;
                            break;
                        }

                        Console.WriteLine("None remove. Try reinserting remove gaz");

                        for (var i = 0; i < equil.Product.NumSpecies[Constants.GAS]; i++) {
                            /* It happen that some species were eliminated in the
                              process even if they should be present in the equilibrium.
                              In such case, we have to reinsert them */
                            if (equil.Product.Coef[Constants.GAS][i] == 0.0) {
                                equil.Product.Coef[Constants.GAS][i] = 1e-6;
                            }
                        }
                        gas_reinserted = true;
                    } else {
                        gas_reinserted = false;
                    }

                    // Restart the loop counter to zero for a new loop
                    k = -1;
                } else {
                    // There is a solution
                    solution_ok = true;
                }
            }

            if (_verbose > 2) {
                MatrixUtils.PrintVec(sol, size);    // print the solution vector
            }

            // compute the new approximation
            NewApproximation(equil, sol, P);

            var convergence_ok = false;

            // verify the convergence
            if (Convergence(equil, sol)) {
                convergence_ok = true;

                if (_verbose > 0) {
                    Console.WriteLine($"Convergence: {k + 1,-2} iteration, {equil.Properties.T:0.000000} deg K");
                }
                gas_reinserted = false;

                // find if a new condensed species should be include or remove
                if (RemoveCondensed(ref equil.Product.NumCondensed, equil) ||
                    IncludeCondensed(ref equil.Product.NumCondensed, equil, sol)) {
                    // new size
                    size = equil.Product.NumElements + equil.Product.NumSpecies[Constants.CONDENSED] + roff;

                    // allocate the memory for the matrix
                    matrix = Utils.Make2DArray<double>(size, size + 1);

                    // allocate the memory for the solution vector
                    sol = new double[size];

                    // haven't converge yet
                    convergence_ok = false;
                }

                // reset the loop counter to compute a new equilibrium
                k = -1;
            } else if (_verbose > 2) {
                Console.WriteLine("The solution doesn't converge\n");
            }

            if (convergence_ok || stop) {
                // when the solution have converge, we could get out of the main loop 
                // if there was problem, the stop flag is set and we also get out 
                break;
            }
        } // end of main loop 

        if (k == ITERATION_MAX) {
            Console.WriteLine();
            Console.WriteLine($"Maximum number of {ITERATION_MAX} iterations attained");
            Console.WriteLine("Don't trust results.");
            return false;
        } else if (stop) {
            Console.WriteLine();
            Console.WriteLine("Problem computing equilibrium...aborted.");
            Console.WriteLine("Don't trust results.");
            return false;
        }

        equil.Product.IsEquilibrium = true;
        ComputeThermoProperties(equil);
        Derivative(equil);

        return true;
    }
}