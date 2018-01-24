[![Build Status](https://travis-ci.org/NeKzor/SourceDemoParser.Net.svg?branch=master)](https://travis-ci.org/NeKzor/SourceDemoParser.Net)
[![Build Version](https://img.shields.io/badge/version-v1.0-yellow.svg)](https://github.com/NeKzor/SourceDemoParser.Net/projects/1)
[![Release Status](https://img.shields.io/github/release/NeKzor/SourceDemoParser.Net/all.svg)](https://github.com/NeKzor/SourceDemoParser.Net/releases)

Parse any protocol version 4 Source Engine demo.

## Overview
- [Main Features](#main-features)
- [C# Documentation](#c--documentation)
  - [Namespaces](#namespaces)
  - [Parsing](#parsing)
  - [Custom Parser](#custom-parser)
  - [Extensions](#extensions)
  - [Adjustments](#adjustments)
  - [Custom Adjustments](#custom-adjustments)
    - [ISourceDemo](#isourcedemo)
    - [Discover, Load & Adjust](#discover-load--adjust)
  - [Parse, Edit & Export](#parse-edit--export)
- [Examples](#examples)

## Main Features
- Multiple parsing modes
- Parses almost everything
- Demo header fix
- Adjustment for special demo rules, defined by speedrunning communities

## C# Docs

### Namespaces

| Namespace | Status | Description |
| --- | :-: | --- |
| [SourceDemoParser.Net](src/SourceDemoParser.Net) | ✖ | SourceDemo, SourceParser etc. |
| [SourceDemoParser.Net.Extensions](src/SourceDemoParser.Net/Extensions) | ✖ | Adjustment, exporting etc. |
| [SourceDemoParser.Net.Extensions.Demos](src/SourceDemoParser.Net/Extensions/Demos) | ✖ | Supported, default adjustments. |

### Parsing
```cs
using SourceDemoParser;

var parser = new SourceParser();
var demo = await parser.ParseFileAsync("rank2.dem");
```

### Custom Parser
```cs
using SourceDemoParser;

// TODO: better example
public class MyParser : SourceParser
{
	public override async Task<IFrame> HandleMessageAsync(BinaryReader br, DemoMessage message)
	{
		var frame = default(IFrame);
		if ((int)message.Type == 10)
		{
			// Your custom parsing logic here
			frame = new MyCustomFrame(br.ReadInt32()) as IFrame;

			// Ignore MessageTypeException
			try
			{
				await base.HandleMessageAsync(br, message);
			}
			catch
			{
			}
		}
		else
		{
			await base.HandleMessageAsync(br, message);
		}

		return frame;
	}
}
```

### Extensions
```cs
using SourceDemoParser.Extensions;

var tickrate = demo.GetTickrate();
var tps = demo.GetTicksPerSecond();
```

### Adjustments
```cs
using SourceDemoParser.Extensions;

// Some games and mods (Portal 2 etc.) have issues when ending a demo
// through a changelevel. To fix the incorrect header (PlaybackTime and
// PlaybackTicks) we take the last positive tick of the parsed messages
await demo.AdjustExact();

// Adjusts demo until a special command. Default standard was defined
// by the SourceRuns community (echo #SAVE#)
await demo.AdjustFlagAsync(saveFlag: "echo #IDEEDIT#");

// Adjustments for specific maps with special rules (see below)
await demo.AdjustAsync();
```

### Custom Adjustments

#### ISourceDemo
```cs
using SourceDemoParser.Extensions;

// Implement ISourceDemo
public class Portal2CustomMapDemo : ISourceDemo
{
	// Set demo folder and tickrate
	public string GameDirectory => "portal2";
	public uint DefaultTickrate => 60u;
	
	// Return boolean for an adjustment
	// Example: Find start tick of a specific map
	[StartAdjustment("sp_gud_mape")]
	public bool SpGudMape_Start(PlayerPosition pos)
	{
		// Search logic with: PlayerPosition
		var destination = new Vector(-723.00f, -2481.00f, 17.00f);
		if ((pos.Old != destination) && (pos.Current != destination))
			return true;
		return false;
	}
	
	// Example: Find end tick of a specific map with negative tick offset
	[EndAdjustment("sp_gud_mape_finale", -1)]
	public bool GgStageTheend_Ending(PlayerCommand cmd)
	{
		// Search logic with: PlayerCommand
		var command = "playvideo_exitcommand_nointerrupt at_credits end_movie credits_video";
		return (cmd.Current == command);
	}
	
	// Example: Find end tick of any map with positive tick offset
	[EndAdjustment(offset: 1)]
	public bool ForSpecialCasesAlwaysCheck_Ending(PlayerCommand cmd)
	{
		var command = "echo SPECIAL_FADEOUT_WITH_VALUE";
		return (cmd.Current.StartsWitch(command));
	}
}
```
##### [More Demo Examples](src/SourceDemoParser.Net/Extensions/Demos)

#### Discover, Load & Adjust
```cs
// Stuff will be cached automatically on success
// Load all default adjustments
await SourceExtensions.DiscoverAsync();

// Load all adjustments of an assembly using System.Reflection:
var result = await SourceExtensions.DiscoverAsync(Assembly.GetEntryAssembly());
if (result) Console.WriteLine("Loaded at least one new adjustment.");

// Or load manually
result = await SourceExtensions.LoadAsync<Portal2CustomMapDemo>();
if (result) Console.WriteLine("Loaded " + nameof(Portal2CustomMapDemo));

// Finally use:
await demo.AdjustAsync();
```

### Parse, Edit & Export
```cs
using SourceDemoParser;
using SourceDemoParser.Extensions;

var parser = new SourceParser();
var demo = await parser.ParseFileAsync("just_a_wr_by_zypeh.dem");
demo.Playbackticks = 1337;
demo.PlaybackTime = 420; 
await demo.ExportFileAsync("h4ck3r.dem");
```

## Examples

### [CLI Tool](src/SourceDemoParser-CLI)
Simple tool for command line interfaces.

Example: `dotnet sdp.dll header segment_42.dem`.

### [SourceDemoParser-DS](src/SourceDemoParser-DS)
[![Showcase](showcase.gif)](src/SourceDemoParser-DS)