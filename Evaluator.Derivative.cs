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

/* derivative.c  -  Fill the mattrix to compute thermochemical derivative
                    relative to logarithm of pressure and temperature */
/* $Id: derivative.c,v 1.1 2000/07/14 00:30:53 antoine Exp $ */
/* Copyright (C) 2000                                                  */
/*    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>                   */
/*    Mark Pinese <pinese@cyberwizards.com.au>                         */
/*                                                                     */
/* Licensed under the GPLv2                                            */

using ImpulseRocketry.LibNum;

namespace ImpulseRocketry.LibPropellantEval;

public partial class Evaluator {
    // Compute the specific_heat of the mixture using thermodynamics derivative with respect to logarithm of temperature
    private double MixtureSpecificHeat(Equilibrium e, double[] sol) {
        var p = e.Product;
        var pr = e.Properties;

        double tmp;
        var cp = 0.0;
        // Compute Cp/R
        for (var i = 0; i < p.NumElements; i++) {
            tmp = 0.0;
            for (var j = 0; j < p.NumSpecies[Constants.GAS]; j++) {
                tmp += p.A[i][j] * p.Coef[Constants.GAS][j] * ThermoList.Enthalpy0(p.Species[Constants.GAS][j], (float)pr.T);
            }

            cp += tmp * sol[i];
        }

        for (var i = 0; i < p.NumSpecies[Constants.CONDENSED]; i++) {
            cp += ThermoList.Enthalpy0(p.Species[Constants.CONDENSED][i], (float)pr.T) * sol[i + p.NumElements];
        }

        tmp = 0.0;
        for (var i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            tmp += p.Coef[Constants.GAS][i] * ThermoList.Enthalpy0(p.Species[Constants.GAS][i], (float)pr.T);
        }

        cp += tmp * sol[p.NumElements + p.NumSpecies[Constants.CONDENSED]];

        cp += MixtureSpecificHeat0(e, pr.T);

        for (var i = 0; i < p.NumSpecies[Constants.GAS]; i++) {
            cp += p.Coef[Constants.GAS][i] * Math.Pow(ThermoList.Enthalpy0(p.Species[Constants.GAS][i], (float)pr.T), 2);
        }

        return cp;
    }

    private int Derivative(Equilibrium e) {
        var p = e.Product;
        var prop = e.Properties;

        // the size of the coefficient matrix
        var size = p.NumElements + p.NumSpecies[Constants.CONDENSED] + 1;

        // allocate the memory for the matrix
        var matrix = Utils.Make2DArray<double>(size, size + 1);

        // allocate the memory for the solution vector
        var sol = new double[size];

        FillTemperatureDerivativeMatrix(matrix, e);

        if (!MatrixUtils.Lu(matrix, sol, size)) {
            Console.WriteLine("The matrix is singular.");
        } else {
            if (_verbose > 2) {
                Console.WriteLine("Temperature derivative results.");
                MatrixUtils.PrintVec(sol, size);
            }

            prop.Cp = MixtureSpecificHeat(e, sol) * Constants.R;
            prop.dV_T = 1 + sol[e.Product.NumElements + e.Product.NumSpecies[Constants.CONDENSED]];
        }

        FillPressureDerivativeMatrix(matrix, e);

        if (!MatrixUtils.Lu(matrix, sol, size)) {
            Console.WriteLine("The matrix is singular.");
        } else {
            if (_verbose > 2) {
                Console.WriteLine("Pressure derivative results.");
                MatrixUtils.PrintVec(sol, size);
            }

            prop.dV_P = sol[e.Product.NumElements + e.Product.NumSpecies[Constants.CONDENSED]] - 1;
        }

        prop.Cv = prop.Cp + e.IterationInfo.N * Constants.R * Math.Pow(prop.dV_T, 2) / prop.dV_P;
        prop.Isex = -(prop.Cp / prop.Cv) / prop.dV_P;
        prop.Vson = Math.Sqrt(1000 * e.IterationInfo.N * Constants.R * e.Properties.T * prop.Isex);

        return 0;
    }

    // Fill the matrix with the coefficient for evaluating derivatives with
    // respect to logarithm of temperature at constant pressure 
    private int FillTemperatureDerivativeMatrix(double[][] matrix, Equilibrium e) {
        double tmp;

        var p = e.Product;
        var pr = e.Properties;

        var idx_cond = p.NumElements;
        var idx_n = p.NumElements + p.NumSpecies[Constants.CONDENSED];
        var idx_T = p.NumElements + p.NumSpecies[Constants.CONDENSED] + 1;

        // Fill the common part
        FillMatrix(matrix, e);

        // del ln(n)/ del ln(T)
        matrix[idx_n][idx_n] = 0.0;

        // Right side
        for (var j = 0; j < p.NumElements; j++) {
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp -= p.A[j][k] * p.Coef[Constants.GAS][k] * ThermoList.Enthalpy0(p.Species[Constants.GAS][k], (float)pr.T);
            }

            matrix[j][idx_T] = tmp;
        }

        for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) { // row
            matrix[j + idx_cond][idx_T] = -ThermoList.Enthalpy0(p.Species[Constants.CONDENSED][j], (float)pr.T);
        }

        tmp = 0.0;
        for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
            tmp -= p.Coef[Constants.GAS][k] * ThermoList.Enthalpy0(p.Species[Constants.GAS][k], (float)pr.T);
        }

        matrix[idx_n][idx_T] = tmp;

        return 0;
    }

    // Fill the matrix with the coefficient for evaluating derivatives with
    // respect to logarithm of pressure at constant temperature 
    private int FillPressureDerivativeMatrix(double[][] matrix, Equilibrium e) {
        double tmp;

        var p = e.Product;

        var idx_cond = p.NumElements;
        var idx_n = p.NumElements + p.NumSpecies[Constants.CONDENSED];
        var idx_T = p.NumElements + p.NumSpecies[Constants.CONDENSED] + 1;

        // Fill the common part
        FillMatrix(matrix, e);

        // del ln(n)/ del ln(T)
        matrix[idx_n][idx_n] = 0.0;

        // Right side
        for (var j = 0; j < p.NumElements; j++) {
            tmp = 0.0;
            for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
                tmp += p.A[j][k] * p.Coef[Constants.GAS][k];
            }

            matrix[j][idx_T] = tmp;
        }

        for (var j = 0; j < p.NumSpecies[Constants.CONDENSED]; j++) { // row
            matrix[j + idx_cond][idx_T] = 0;
        }

        tmp = 0.0;
        for (var k = 0; k < p.NumSpecies[Constants.GAS]; k++) {
            tmp += p.Coef[Constants.GAS][k];
        }

        matrix[idx_n][idx_T] = tmp;

        return 0;
    }
}