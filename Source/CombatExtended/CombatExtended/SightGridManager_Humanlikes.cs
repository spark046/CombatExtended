﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended
{
    public class SightGridManager_Humanlikes : SightGridManager<Pawn>
    {
        public SightGridManager_Humanlikes(SightTracker tracker, int bucketCount = 20, int minUpdateInterval = 10, int maxUpdateInterval = 25)
            : base(tracker, bucketCount, minUpdateInterval, maxUpdateInterval)
        {
        }       
      
        protected override bool Valid(Pawn pawn)
        {            
            return base.Valid(pawn) && !pawn.Dead && !pawn.WorkTagIsDisabled(WorkTags.Violent) && pawn.RaceProps.Humanlike && pawn.RaceProps.intelligence == Intelligence.Humanlike;
        }        

        protected override int GetSightRange(IThingSightRecord record)
        {
            Pawn pawn = record.thing;
            ThingWithComps weapon = pawn.equipment.Primary;
            if (weapon == null || !weapon.def.IsRangedWeapon)
                return -1;

            float range;
            range = Mathf.Min(weapon.def.verbs?.Max(v => v.range) ?? -1, 62f) * PerformanceFactor;
            range = (range + record.carryRange) / (1 + record.carry);
            if (range < MinRange)
                return -1;

            SkillRecord skillRecord = pawn.skills?.GetSkill(SkillDefOf.Shooting) ?? null;
            if (record != null)
            {
                float skill = skillRecord.Level;
                if (map.IsNightTime())
                    skill = Mathf.Max(skill - (1 - pawn.GetStatValue(CE_StatDefOf.NightVisionEfficiency)) * 4, 0f);
                range *= Mathf.Clamp(skill / 7.5f, 0.85f, 1.75f);
            }
            return Mathf.CeilToInt(range);
        }

        protected override bool Skip(IThingSightRecord record)
        {
            Pawn pawn = record.thing;
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
                return true;
            if (GenTicks.TicksGame - pawn.needs?.rest?.lastRestTick <= 30)
                return true;
            if (pawn.equipment?.equipment == null)
                return true;
            return false;
        }        

        protected override IEnumerable<Pawn> ThingsInRange(IntVec3 position, float range) => position.PawnsInRange(map, range);
    }
}

