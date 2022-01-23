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

/* performance.c  -  Compute performance caracteristic of a motor
                     considering equilibrium                      */
/* $Id: performance.c,v 1.2 2000/07/19 02:13:03 antoine Exp $ */
/* Copyright (C) 2000                                                  */
/*    Antoine Lefebvre <antoine.lefebvre@polymtl.ca>                   */
/*    Mark Pinese  <pinese@cyberwizards.com.au>                        */
/*                                                                     */
/* Licensed under the GPLv2                                            */

namespace ImpulseRocketry.LibPropellantEval;

public partial class Evaluator {

    public const int TEMP_ITERATION_MAX = 8;
    public const int PC_PT_ITERATION_MAX = 5;
    public const int PC_PE_ITERATION_MAX = 6;

    // Entropy of the product at the exit pressure and temperature
    private double ProductEntropyExit(Equilibrium e, double pressure, double temp) {
        double t = e.Properties.T;
        double p = e.Properties.P;
        e.Properties.T = temp;
        e.Properties.P = pressure;
        var ent = ProductEntropy(e); // entropy at the new pressure
        e.Properties.T = t;
        e.Properties.P = p;
        return ent;
    }

    // Enthalpy of the product at the exit temperature
    private double ProductEnthalpyExit(Equilibrium e, double temp) {
        double t = e.Properties.T;
        e.Properties.T = temp;
        var enth = ProductEnthalpy(e);
        e.Properties.T = t;
        return enth;
    }

    // The temperature could be found by entropy conservation with a
    // specified pressure
    private double ComputeTemperature(Equilibrium e, double pressure, double p_entropy) {
        double delta_lnt;

        // The first approximation is the chamber temperature
        var temperature = e.Properties.T;

        var i = 0;
        do {
            delta_lnt = (p_entropy - ProductEntropyExit(e, pressure, temperature)) / MixtureSpecificHeat0(e, temperature);

            temperature = Math.Exp(Math.Log(temperature) + delta_lnt);

            i++;
        } while (Math.Abs(delta_lnt) >= 0.5e-4 && i < TEMP_ITERATION_MAX);

        if (i == TEMP_ITERATION_MAX) {
            Console.Error.WriteLine($"Temperature do not converge in {TEMP_ITERATION_MAX} iterations. Don't thrust results.");
        }

        return temperature;
    }

    public bool FrozenPerformance(Equilibrium[] es, ExitCondition exit_type, double value) {
        double sound_velocity;
        double flow_velocity;
        double pc_pt;            // Chamber pressure / Throat pressure
        double pc_pe;            // Chamber pressure / Exit pressure
        double log_pc_pe;        // log(pc_pe)
        double ae_at;            // Exit aera / Throat aera
        double cp_cv;
        double chamber_entropy;
        double exit_pressure;

        var e = es[0];
        var t = es[1]; // throat equilibrium
        var ex = es[2]; // exit equilibrium

        // find the equilibrium composition in the chamber
        if (!e.Product.IsEquilibrium) {
            if (!Equilibrium(e, ProblemType.HP)) {
                Console.WriteLine("No equilibrium, performance evaluation aborted.");
                return false;
            }
        }

        // Simplification due to frozen equilibrium
        e.Properties.dV_T = 1.0;
        e.Properties.dV_P = -1.0;
        e.Properties.Cp = MixtureSpecificHeat0(e, e.Properties.T) * Constants.R;
        e.Properties.Cv = e.Properties.Cp - e.IterationInfo.N * Constants.R;
        e.Properties.Isex = e.Properties.Cp / e.Properties.Cv;

        ComputeThermoProperties(e);

        chamber_entropy = ProductEntropy(e);

        // begin computation of throat caracteristic
        e.CopyTo(t);

        cp_cv = e.Properties.Cp / e.Properties.Cv;

        // first estimate of Pc/Pt
        pc_pt = Math.Pow(cp_cv / 2 + 0.5, cp_cv / (cp_cv - 1));

        var i = 0;
        do {
            t.Properties.T = ComputeTemperature(t, e.Properties.P / pc_pt,
                                                chamber_entropy);

            // Cp of the combustion point assuming frozen
            t.Properties.Cp = MixtureSpecificHeat0(e, t.Properties.T) * Constants.R;
            // Cv = Cp - nR  (for frozen)
            t.Properties.Cv = t.Properties.Cp - t.IterationInfo.N * Constants.R;
            t.Properties.Isex = t.Properties.Cp / t.Properties.Cv;

            ComputeThermoProperties(t);

            sound_velocity = Math.Sqrt(1000 * e.IterationInfo.N * Constants.R * t.Properties.T *
                                t.Properties.Isex);

            flow_velocity = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                    ProductEnthalpyExit(e, t.Properties.T) *
                                    Constants.R * t.Properties.T));

            pc_pt /= (1 + ((Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))
                                    / (1000 * (t.Properties.Isex + 1) *
                                    t.IterationInfo.N * Constants.R * t.Properties.T)));
            i++;
        } while ((Math.Abs((Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))
                / Math.Pow(flow_velocity, 2)) > 0.4e-4) && (i < PC_PT_ITERATION_MAX));

        if (i == PC_PT_ITERATION_MAX) {
            Console.Error.WriteLine($"Throat pressure do not converge in {PC_PT_ITERATION_MAX} iterations. Don't thrust results");
        }

        //printf("%d iterations to evaluate throat pressure.\n", i);

        t.Properties.P = e.Properties.P / pc_pt;
        t.Performance.Isp = t.Properties.Vson = sound_velocity;

        // Now compute exit properties
        e.CopyTo(ex);

        if (exit_type == ExitCondition.PRESSURE) {
            exit_pressure = value;
        } else {
            ae_at = value;

            // Initial estimate of pressure ratio
            if (exit_type == ExitCondition.SUPERSONIC_AREA_RATIO) {
                if ((ae_at > 1.0) && (ae_at < 2.0)) {
                    log_pc_pe = Math.Log(pc_pt) + Math.Sqrt(3.294 * Math.Pow(ae_at, 2) + 1.535 * Math.Log(ae_at));
                } else if (ae_at >= 2.0) {
                    log_pc_pe = t.Properties.Isex + 1.4 * Math.Log(ae_at);
                } else {
                    Console.WriteLine("Aera ratio out of range ( < 1.0 )");
                    return true;
                }
            } else if (exit_type == ExitCondition.SUBSONIC_AREA_RATIO) {
                if ((ae_at > 1.0) && (ae_at < 1.09)) {
                    log_pc_pe = 0.9 * Math.Log(pc_pt) /
                    (ae_at + 10.587 * Math.Pow(Math.Log(ae_at), 3) + 9.454 * Math.Log(ae_at));
                } else if (ae_at >= 1.09) {
                    log_pc_pe = Math.Log(pc_pt) /
                    (ae_at + 10.587 * Math.Pow(Math.Log(ae_at), 3) + 9.454 * Math.Log(ae_at));
                } else {
                    Console.WriteLine("Aera ratio out of range ( < 1.0 )");
                    return true;
                }
            } else {
                return false;
            }

            // Improved the estimate
            i = 0;
            do {
                pc_pe = Math.Exp(log_pc_pe);
                ex.Properties.P = exit_pressure = e.Properties.P / pc_pe;
                ex.Properties.T = ComputeTemperature(e, exit_pressure,
                                                        chamber_entropy);
                // Cp of the combustion point assuming frozen
                ex.Properties.Cp = MixtureSpecificHeat0(e, ex.Properties.T) * Constants.R;
                // Cv = Cp - nR  (for frozen)
                ex.Properties.Cv = ex.Properties.Cp - ex.IterationInfo.N * Constants.R;
                ex.Properties.Isex = ex.Properties.Cp / ex.Properties.Cv;

                ComputeThermoProperties(ex);

                sound_velocity = Math.Sqrt(1000 * ex.IterationInfo.N * Constants.R * ex.Properties.T *
                                        ex.Properties.Isex);

                ex.Performance.Isp =
                    flow_velocity = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                            ProductEnthalpyExit(e, ex.Properties.T) *
                                            Constants.R * ex.Properties.T));

                ex.Performance.AeAt =
                    (ex.Properties.T * t.Properties.P * t.Performance.Isp) /
                    (t.Properties.T * ex.Properties.P * ex.Performance.Isp);

                log_pc_pe += (ex.Properties.Isex * Math.Pow(flow_velocity, 2) /
                             (Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))) *
                             (Math.Log(ae_at) - Math.Log(ex.Performance.AeAt));

                i++;

            } while ((Math.Abs(log_pc_pe - Math.Log(pc_pe)) > 0.00004) &&
                    (i < PC_PE_ITERATION_MAX));

            if (i == PC_PE_ITERATION_MAX) {
                Console.Error.WriteLine($"Exit pressure do not converge in {PC_PE_ITERATION_MAX} iterations. Don't thrust results");
            }

            //printf("%d iterations to evaluate exit pressure.\n", i);

            pc_pe = Math.Exp(log_pc_pe);
            exit_pressure = e.Properties.P / pc_pe;
        }

        ex.Properties.T = ComputeTemperature(e, exit_pressure, chamber_entropy);
        // We must check if the exit temperature is more than 50 K lower
        // than any transition temperature of condensed species.
        // In this case the results are not good and must be reject.

        ex.Properties.P = exit_pressure;
        ex.Performance.Isp = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                            ProductEnthalpyExit(e, ex.Properties.T) *
                                            Constants.R * ex.Properties.T));


        // units are (m/s/atm)
        ex.Performance.ADotm = 1000 * Constants.R * ex.Properties.T * ex.IterationInfo.N /
            (ex.Properties.P * ex.Performance.Isp);

        // Cp of the combustion point assuming frozen
        ex.Properties.Cp = MixtureSpecificHeat0(e, ex.Properties.T) * Constants.R;
        // Cv = Cp - nR  (for frozen)
        ex.Properties.Cv = ex.Properties.Cp - ex.IterationInfo.N * Constants.R;
        ex.Properties.Isex = ex.Properties.Cp / ex.Properties.Cv;

        ComputeThermoProperties(ex);

        ex.Properties.Vson = Math.Sqrt(1000 * e.IterationInfo.N * Constants.R * ex.Properties.T *
                                    e.Properties.Isex);


        t.Performance.ADotm = 1000 * Constants.R * t.Properties.T
            * t.IterationInfo.N / (t.Properties.P * t.Performance.Isp);
        t.Performance.AeAt = 1.0;
        t.Performance.Cstar = e.Properties.P * t.Performance.ADotm;
        t.Performance.Cf = t.Performance.Isp /
            (e.Properties.P * t.Performance.ADotm);
        t.Performance.Ivac = t.Performance.Isp + t.Properties.P
            * t.Performance.ADotm;

        ex.Performance.AeAt =
            (ex.Properties.T * t.Properties.P * t.Performance.Isp) /
            (t.Properties.T * ex.Properties.P * ex.Performance.Isp);
        ex.Performance.Cstar = e.Properties.P * t.Performance.ADotm;
        ex.Performance.Cf = ex.Performance.Isp /
            (e.Properties.P * t.Performance.ADotm);
        ex.Performance.Ivac = ex.Performance.Isp + ex.Properties.P
            * ex.Performance.ADotm;

        return true;
    }

    public bool ShiftingPerformance(Equilibrium[] es, ExitCondition exit_type, double value) {
        double sound_velocity;
        double flow_velocity;
        double pc_pt;
        double pc_pe;
        double log_pc_pe;
        double ae_at;
        double chamber_entropy;
        double exit_pressure;

        var e = es[0];
        var t = es[1]; // throat equilibrium
        var ex = es[2]; // throat equilibrium

        // find the equilibrium composition in the chamber
        if (!e.Product.IsEquilibrium) {
            // if the equilibrium have not already been compute
            if (!Equilibrium(e, ProblemType.HP)) {
                Console.WriteLine("No equilibrium, performance evaluation aborted.");
                return false;
            }
        }

        // Begin by first aproximate the new equilibrium to be
        // the same as the chamber equilibrium
        e.CopyTo(t);

        chamber_entropy = ProductEntropy(e);

        // Computing throat condition
        // Approximation of the throat pressure
        pc_pt = Math.Pow(t.Properties.Isex / 2 + 0.5, t.Properties.Isex / (t.Properties.Isex - 1));

        t.Entropy = chamber_entropy;

        var i = 0;
        do {
            t.Properties.P = e.Properties.P / pc_pt;

            // We must compute the new equilibrium each time
            if (!Equilibrium(t, ProblemType.SP)) {
                Console.WriteLine("No equilibrium, performance evaluation aborted.");
                return false;
            }

            sound_velocity = Math.Sqrt(1000 * t.IterationInfo.N * Constants.R * t.Properties.T * t.Properties.Isex);

            flow_velocity = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                        ProductEnthalpy(t) * Constants.R * t.Properties.T));

            pc_pt /= (1 + ((Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))
                    / (1000 * (t.Properties.Isex + 1) * t.IterationInfo.N * Constants.R *
                    t.Properties.T)));
            i++;
        } while ((Math.Abs((Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))
                        / Math.Pow(flow_velocity, 2)) > 0.4e-4) &&
                (i < PC_PT_ITERATION_MAX));

        if (i == PC_PT_ITERATION_MAX) {
            Console.Error.WriteLine($"Throat pressure do not converge in {PC_PT_ITERATION_MAX} iterations. Don't thrust results.");
        }

        //printf("%d iterations to evaluate throat pressure.\n", i);

        t.Properties.P = e.Properties.P / pc_pt;
        t.Properties.Vson = sound_velocity;
        t.Performance.Isp = sound_velocity;

        t.Performance.ADotm = 1000 * Constants.R *
            t.Properties.T * t.IterationInfo.N /
            (t.Properties.P * t.Performance.Isp);

        e.CopyTo(ex);

        if (exit_type == ExitCondition.PRESSURE) {
            exit_pressure = value;
        } else {
            ae_at = value;

            // Initial estimate of pressure ratio
            if (exit_type == ExitCondition.SUPERSONIC_AREA_RATIO) {
                if ((ae_at > 1.0) && (ae_at < 2.0)) {
                    log_pc_pe = Math.Log(pc_pt) + Math.Sqrt(3.294 * Math.Pow(ae_at, 2) + 1.535 * Math.Log(ae_at));
                } else if (ae_at >= 2.0) {
                    log_pc_pe = t.Properties.Isex + 1.4 * Math.Log(ae_at);
                } else {
                    Console.WriteLine("Aera ratio out of range ( < 1.0 )");
                    return true;
                }
            } else if (exit_type == ExitCondition.SUBSONIC_AREA_RATIO) {
                if ((ae_at > 1.0) && (ae_at < 1.09)) {
                    log_pc_pe = 0.9 * Math.Log(pc_pt) /
                    (ae_at + 10.587 * Math.Pow(Math.Log(ae_at), 3) + 9.454 * Math.Log(ae_at));
                } else if (ae_at >= 1.09) {
                    log_pc_pe = Math.Log(pc_pt) /
                    (ae_at + 10.587 * Math.Pow(Math.Log(ae_at), 3) + 9.454 * Math.Log(ae_at));
                } else {
                    Console.WriteLine("Aera ratio out of range ( < 1.0 )");
                    return true;
                }
            } else {
                return false;
            }

            // Improved the estimate
            ex.Entropy = chamber_entropy;
            i = 0;
            do {
                pc_pe = Math.Exp(log_pc_pe);
                exit_pressure = e.Properties.P / pc_pe;
                ex.Properties.P = exit_pressure;

                // Find the exit equilibrium
                if (!Equilibrium(ex, ProblemType.SP)) {
                    Console.WriteLine("No equilibrium, performance evaluation aborted.");
                    return false;
                }

                sound_velocity = ex.Properties.Vson;

                ex.Performance.Isp =
                    flow_velocity = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                            ProductEnthalpy(ex) * Constants.R * ex.Properties.T));

                ex.Performance.AeAt =
                    (ex.Properties.T * t.Properties.P * t.Performance.Isp) /
                    (t.Properties.T * ex.Properties.P * ex.Performance.Isp);

                log_pc_pe += 
                    (ex.Properties.Isex * Math.Pow(flow_velocity, 2) /
                    (Math.Pow(flow_velocity, 2) - Math.Pow(sound_velocity, 2))) *
                    (Math.Log(ae_at) - Math.Log(ex.Performance.AeAt));
                i++;
            } while ((Math.Abs((log_pc_pe - Math.Log(pc_pe))) > 0.00004) &&
                    (i < PC_PE_ITERATION_MAX));

            if (i == PC_PE_ITERATION_MAX) {
                Console.Error.WriteLine($"Exit pressure do not converge in {PC_PE_ITERATION_MAX} iteration. Don't thrust results.");
            }

            //printf("%d iterations to evaluate exit pressure.\n", i);

            pc_pe = Math.Exp(log_pc_pe);
            exit_pressure = e.Properties.P / pc_pe;
        }

        ex.Entropy = chamber_entropy;
        ex.Properties.P = exit_pressure;

        // Find the exit equilibrium
        if (!Equilibrium(ex, ProblemType.SP)) {
            Console.WriteLine("No equilibrium, performance evaluation aborted.");
            return false;
        }

        flow_velocity = Math.Sqrt(2000 * (ProductEnthalpy(e) * Constants.R * e.Properties.T -
                                    ProductEnthalpy(ex) * Constants.R * ex.Properties.T));

        ex.Performance.Isp = flow_velocity;

        ex.Performance.ADotm = 1000 * Constants.R *
            ex.Properties.T * ex.IterationInfo.N /
            (ex.Properties.P * ex.Performance.Isp);

        t.Performance.AeAt = 1.0;
        t.Performance.Cstar = e.Properties.P * t.Performance.ADotm;
        t.Performance.Cf = t.Performance.Isp /
            (e.Properties.P * t.Performance.ADotm);
        t.Performance.Ivac = t.Performance.Isp + t.Properties.P
            * t.Performance.ADotm;

        ex.Performance.AeAt =
            (ex.Properties.T * t.Properties.P * t.Performance.Isp) /
            (t.Properties.T * ex.Properties.P * ex.Performance.Isp);
        ex.Performance.Cstar = e.Properties.P * t.Performance.ADotm;
        ex.Performance.Cf = ex.Performance.Isp /
            (e.Properties.P * t.Performance.ADotm);
        ex.Performance.Ivac = ex.Performance.Isp + ex.Properties.P
            * ex.Performance.ADotm;

        return true;
    }
}
