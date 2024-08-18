# Crash Plugin SDK

Welcome to Crash Plugin Development.

Crash Plugins is currently in beta. The API is liable to change based on user feedback and will likely not be finalised and stable until 2025. Please don't rely on any Crash Plugins for now.

---

That being said, if you're here to have fun, you'll be right at home!

## Getting Started

Clone or fork the CrashPluginExample from https://github.com/crashcloud/CrashPluginExample.

### Concepts

**Plugin assembly**

The Crash Plugin must be built into a `.mup` assembly. As some plugins are created for Rhino using C++ I made Crash require a separate DLL so these Plugins can create a separate C# DLL to consume anything required. Whilst it might make sense for some plugins to include Crash inside their main plugin, I'd like to make Crash separate and available for every plugin. You should include these in your `.yak` package (or at least distribute next to your `.rhp`) so that Crash can find them.

**Changes**

Changes are how we capture a change during Rhinos runtime. This can be a box being transformed, a Grasshopper component being added to the canvas, or anything you need to communicate to other users of the Shared Model. These changes can even be completely independent of the Rhino Doc.

**Change Definition**

The Change Definition describes how the changes are drawn, if at all, how they are categorised and what actions can be taken with the changes.

**Change Create Action**

Change Create Actions define the circumstances in which a Change is created. This could be a Box being moved, deleted, selected, or even a completely unique event in your Plugin!

**Change Receive Action**

Change Receive Actions define how changes get created locally when a Change is received from the server that matches the description of the Change Definition.

**Events**
If you do want to subscribe to custom events and let crash know, you MUST forward the event to `crashDoc.Dispatcher.NotifyServerAsync`. It is recommended, if you can, to create your own custom event args as this will make it easier for you to capture that change in your custom `ChangeCreateAction`. DO NOT subscribe to the default Rhino Events. These are already captured by Crash. If Crash is missing a Rhino Event you would like subscribed to, please open a PR and add it.

# Contributing

If you like or dislike the way the plugin API works, have requests, questions or more, please post on the Discourse Forums under the `Multi-user` category https://discourse.mcneel.com/c/plug-ins/multi-user/163.
