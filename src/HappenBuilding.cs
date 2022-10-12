﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Atmo.Atmod;
using static Atmo.HappenTrigger;
using static Atmo.HappenBuilding;
using static Atmo.API;

namespace Atmo;
/// <summary>
/// Manages happens' initialization and builtin behaviours.
/// </summary>
internal static class HappenBuilding
{
    internal static void NewEvent(Happen ha)
    {
        AddDefaultCallbacks(ha);
        if (MNH_invl is null) return;
        foreach (var cb in MNH_invl)
        {
            try
            {
                cb?.Invoke(ha);
            }
            catch (Exception ex)
            {
                inst.Plog.LogError($"Error invoking event factory {cb} for {ha}:\n{ex}");
            }
        }
        //API_MakeNewHappen?.Invoke(ha);
    }
    internal static void AddDefaultCallbacks(Happen ha)
    {
        //todo: add default cases
    }
    /// <summary>
    /// Creates a new trigger with given ID, arguments using provided <see cref="RainWorldGame"/>.
    /// </summary>
    /// <param name="id">Name or ID</param>
    /// <param name="args">Optional arguments</param>
    /// <param name="rwg">game instance</param>
    /// <returns>Resulting trigger; an <see cref="Always"/> if something went wrong.</returns>
    internal static HappenTrigger CreateTrigger(
        string id,
        string[] args,
        RainWorldGame rwg)
    {
#warning untested
        HappenTrigger? res = null;
        switch (id)
        {
            case "untilrain":
            case "beforerain":
                {
                    int.TryParse(args.AtOr(0, "0"), out var delay);
                    res = new BeforeRain(rwg, delay);
                }
                break;
            case "afterrain":
                {
                    int.TryParse(args.AtOr(0, "0"), out var delay);
                    res = new AfterRain(rwg, delay);
                }
                break;
            case "everyx":
            case "every":
                {
                    var def = "40";
                    int.TryParse(args.AtOr(0, "40"), out var period);
                    res = new EveryX(period);
                }
                break;
            case "maybe":
            case "chance":
                {
                    float.TryParse(args.AtOr(0, "0.5"), out var ch);
                    res = new Maybe(ch);
                }
                break;
            case "flicker":
                {
                    int[] argsp = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        int.TryParse(args.AtOr(i, "300"), out argsp[i]);
                    }
                    bool startOn = trueStrings.Contains(args.AtOr(4, "1").ToLower());
                    res = new Flicker(argsp[0], argsp[1], argsp[2], argsp[3], startOn);
                }
                break;
            case "karma":
                res = new OnKarma(rwg, args);
                break;
        }
        
        if (MNT_invl is null) goto finish;
        foreach (Create_RawTriggerFactory? inv in MNT_invl)
        {
            if (res is not null) break;
            res ??= inv?.Invoke(id, args, rwg);
        }
        finish:
        res ??= new Always();
        return res;
    }
}
