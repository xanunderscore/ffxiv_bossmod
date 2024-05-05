﻿namespace BossMod.Endwalker.Ultimate.TOP;

[ConfigDisplay(Order = 0x210, Parent = typeof(EndwalkerConfig))]
public class TOPConfig() : CooldownPlanningConfigNode(90)
{
    [PropertyDisplay("P1 Program Loop: assignments (G1 CW from NW, G2 CCW by default, in case of conflict 'lower' number flexes)")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (global): M1>M2>MT>OT>R1>R2>H1>H2", [5, 4, 1, 0, 7, 6, 3, 2])]
    public GroupAssignmentUnique P1ProgramLoopAssignments = new() { Assignments = [5, 4, 1, 0, 7, 6, 3, 2] };

    [PropertyDisplay("P1 Program Loop: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
    public bool P1ProgramLoopGlobalPriority = true;

    [PropertyDisplay("P1 Pantokrator: assignments (G1 N, G2 S, adjust CW by default, in case of conflict 'lower' number flexes)")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (light parties): flex T>M>R", [0, 4, 3, 7, 1, 5, 2, 6])]
    public GroupAssignmentUnique P1PantokratorAssignments = new() { Assignments = [0, 4, 3, 7, 1, 5, 2, 6] };

    [PropertyDisplay("P1 Pantokrator: group positions")]
    [PropertyCombo("North/South", "NE/SW")]
    public bool P1PantokratorNESW = false;

    [PropertyDisplay("P1 Pantokrator: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
    public bool P1PantokratorGlobalPriority = false;

    [PropertyDisplay("P2 Party Synergy: assignments (G1 left, G2 right if looking at eye, in case of conflict 'lower' number flexes)")]
    [GroupDetails(["G1 prio1", "G1 prio2", "G1 prio3", "G1 prio4", "G2 prio1", "G2 prio2", "G2 prio3", "G2 prio4"])]
    [GroupPreset("LPDU (light parties): flex R>M>H", [3, 7, 2, 6, 1, 5, 0, 4])]
    public GroupAssignmentUnique P2PartySynergyAssignments = new() { Assignments = [3, 7, 2, 6, 1, 5, 0, 4] };

    [PropertyDisplay("P2 Party Synergy: use global priority instead - consider G1 lower-numbered than G2 (so G1 more likely to flex)")]
    public bool P2PartySynergyGlobalPriority = false;

    [PropertyDisplay("P2 Party Synergy: G2 order for Remote Glitch (far tether)")]
    [PropertyCombo("GPOB (only B and G swap)", "GOPB (reverse order)")]
    public bool P2PartySynergyG2ReverseAll = true;

    [PropertyDisplay("P2 Party Synergy: Swap priority if both stacks are in the same group")]
    [PropertyCombo("Northernmost pair", "Southernmost pair")]
    public bool P2PartySynergyStackSwapSouth = true;

    [PropertyDisplay("P3 Intermission: spread/stack spot assignments, from West to East")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("LPDU (RMTH HTMR)", [2, 5, 3, 4, 1, 6, 0, 7])]
    public GroupAssignmentUnique P3IntermissionAssignments = new() { Assignments = [2, 5, 3, 4, 1, 6, 0, 7] };

    [PropertyDisplay("P3 Intermission: spread/stack position")]
    [PropertyCombo("Stacks S, spreads N", "Stacks N, spreads S")]
    public bool P3IntermissionStacksNorth = true;

    [PropertyDisplay("P3 Monitors: priority, from North to South")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("LPDU (HTMR)", [2, 3, 0, 1, 4, 5, 6, 7])]
    public GroupAssignmentUnique P3MonitorsAssignments = new() { Assignments = [2, 3, 0, 1, 4, 5, 6, 7] };

    [PropertyDisplay("P3 Monitors: position for safe side monitor")]
    [PropertyCombo("North (LPDU)", "South (Aether)")]
    public bool P3LastMonitorSouth = false;

    [PropertyDisplay("P4 Wave Cannon: priority, from North to South (assuming south flex)")]
    [GroupDetails(["W1", "E1", "W2", "E2", "W3", "E3", "W4", "E4"])]
    [GroupPreset("LPDU (TRHM)", [0, 1, 4, 5, 6, 7, 2, 3])]
    public GroupAssignmentUnique P4WaveCannonAssignments = new() { Assignments = [0, 1, 4, 5, 6, 7, 2, 3] };
}
