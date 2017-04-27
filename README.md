# Introduction

This tutorial will teach how to create a simple "Hello World!" program in Unity with Entitas. The aim is to familiarize you with the different pieces that make up an Entitas program, how they interact with eachother and how to structure your code for an Entitas project. This should help you get more out of the various [Example Projects](https://github.com/sschmid/Entitas-CSharp/wiki/Example-projects). 

This will seem like an awful lot of work for such a simple program, but my intention is to show you the "Entitas Way". The benefit of doing things this way may not be obvious at first, but a little imagination should allow you to see how structuring your code like this can allow you to add and remove functionality from your game with relatively little pain. To this end I've included some bonus steps that extend the functionality slightly. You should be able to see how easy it is to plug them into our existing game once we've done the initial legwork to set up the project.

### Unity Project Files
The finished unity project can be downloaded from github [here](https://github.com/FNGgames/Entitas-Hello-World-Unity-Example)

# Step 1 - Install Entitas

1. Create a new Unity Project.
2. Download [**Entitas-Unity.zip**](https://github.com/sschmid/Entitas-CSharp/blob/master/Build/deploy/Entitas-Unity.zip?raw=true). 
3. Unzip and move the `Entitas` folder into your project's Assets folder. Feel free to drop them in a sub folder to keep your project organised. 
4. Create a folder for your game's source code (e.g. "Source").
5. Create a folder called "Generated" inside the folder you just created.

# Step 2 - Generate Contexts

Before you create any components or systems you must point Entitas to the folder you where you want your generated code to go. Open the menu Entitas->Preferences and browse to the folder we just created for Generated code. 

It's also time to define our contexts. In this example we will only use the Game context, but it is okay to keep the default GameState and Input contexts as they are. set Data Providers, Code Generators, and Post Processors to "Everything". Click Generate. You should now have some new folders and files in your generated folder and a message in the console letting you know what's been done. Now we have what we need to start writing components.

![Folder Structure](http://i.imgur.com/ltUYXcU.png)

*The desired folder structure after code generation*

# Step 3 - Create your first component

For this example we will only need one component. It's going to store the message we want to print to the console. Let's create create a new folder inside "Source" called "Components". Create a new C# script called DebugMessageComponent and clear the code Unity has generated for you.

**DebugMessageComponent.cs**
```csharp
using Entitas;

[Game]
public class DebugMessageComponent : IComponent 
{    
    public string message;
}
```

Save your file, go back to Unity, wait for the compiler to complete, then click Generate again. You should now have a file inside your generated folder called GameDebugMessageComponent.cs (in Generated -> Game -> Components).  

# Step 4 - Create your first System

We need a system to listen out for entities with this component added. We don't need it to update every frame, and we only care when an entity has been added - after that we can forget about it. ReactiveSystems are perfect for this.

Our first system then is going to be a ReactiveSystem that operates on the Game context. When we notice that an entity has had a DebugMessageComponent added to it, we want to print that message to the log.

**DebugMessageSystem.cs**
```csharp
using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class DebugMessageSystem : ReactiveSystem<GameEntity>
{
    public DebugMessageSystem(Contexts contexts) : base(contexts.game)
    {
    }

    protected override Collector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        // we only care about entities with DebugMessageComponent 
        return context.CreateCollector(GameMatcher.DebugMessage);
    }

    protected override bool Filter(GameEntity entity)
    {
        // good practice to perform a final check in case 
        // the entity has been altered in a different system.
        return entity.hasDebugMessage;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        // this is the list of entities that meet our conditions
        foreach (var e in entities)
        { 
            // we can safely access their DebugMessage component
            // then grab the string data and print it
            Debug.Log(e.debugMessage.message);
        }
    }
}
```

# Step 5 - Create a "Hello World!" System

We now have a component to hold the message data and a system to print the message whenever that component is added to an entity. We now need to create a system to generate the "Hello World!" message. We'll use an Initialize System since we want it to be created at the start of our program.

**HelloWorldSystem.cs**
```csharp
using Entitas;

public class HelloWorldSystem : IInitializeSystem
{ 
    // always handy to keep a reference to the context 
    // we're going to be interacting with it
    readonly GameContext _context;

    public HelloWorldSystem(Contexts contexts)
    { 
        // get the context from the constructor
        _context = contexts.game;
    }

    public void Initialize()
    {
        // create an entity and give it a DebugMessageComponent with
        // the text "Hello World!" as its data
        _context.CreateEntity().AddDebugMessage("Hello World!");
    }
}
```

# Step 6 - Bring your systems together into a Feature

Features are there to keep your systems organised. They also provide neat visual debugging tools for your systems and keep them visually separated for inspection in your Unity hierarchy. Let's put our two systems together into a feature now. The order in which we add them will define the order in which they get executed when the program runs. Features require that you implement a constructor, where you can use the `Add()` method to add your systems.

**TutorialSystems.cs**
```csharp
using Entitas;

public class TutorialSystems : Feature
{
    public TutorialSystems(Contexts contexts) : base ("Tutorial Systems")
    {
        Add(new HelloWorldSystem(contexts));
        Add(new DebugMessageSystem(contexts));
    }
}
```

# Step 7 - Putting it all together

To make all this code actually execute we need to create a `MonoBehaviour` that we can add to an object in our Unity hierarchy. In your Source folder, create a new C# script and name it `GameController.cs`. This is our point of entry. It's responsible for creating, initializing and executing the systems. 

**GameController.cs**
```csharp
using Entitas;
using UnityEngine;

public class GameController : MonoBehaviour
{
    Systems _systems;

    void Start()
    {
        // get a reference to the contexts
        var contexts = Contexts.sharedInstance;
        
        // create the systems by creating individual features
        _systems = new Feature("Systems")
            .Add(new TutorialSystems(contexts));

        // call Initialize() on all of the IInitializeSystems
        _systems.Initialize();
    }

    void Update()
    {
        // call Execute() on all the IExecuteSystems and 
        // ReactiveSystems that were triggered last frame
        _systems.Execute();
        // call cleanup() on all the ICleanupSystems
        _systems.Cleanup();
    }
}
```

Once you've saved your script, create a new empty GameObject in your hierarchy and add your `GameController.cs` to it. Save your scene and press play. You should see "Hello World!" in your console.

![Success!](http://i.imgur.com/L00e3vg.png)

*Success!*

# Bonus steps

## Fun with Reactive Systems

With your game running, open up the DontDestroyOnLoad object in the hierarchy. You should be able to quickly navigate to the entity you just created. You should be able to see it's DebugMessageComponent with the "Hello World!" message string. Take a look at what happens when you type into that field. 

We've set up our message logging system to react to changes in DebugMesage components, every time you type into the field, the component is replaced and the reactive system is triggered. Now try removing the component and adding it again. Try clicking on the parent object and creating a new entity and adding the component to it. Notice how our logging system handles everything you are doing with ease. 

![Inspecting Components](http://i.imgur.com/KAKthIK.png)

*Components in the inspector*

## Cleanup System

You've come this far, how about we add a couple of extra systems to the example? We already know that we don't need the components to stick around after we've used them. Let's implement a system that gets rid of them after the other systems have finished running.

Here we'll create an `ICleanupSystem` and we'll use a `Group` to keep track of entities with DebugMessages added to them. In our GameController we call `Cleanup()` after `Execute()` so we know that deleting these entities will not interfere with the Execute and Reactive systems that operate on them. In this way we could add more systems in the future that handle our messages in different ways (e.g. printing to a log file or sending them as emails). Thus we don't want our first messaging system to be responsible for destroying these entities, since that would interfere with the systems we have planned for the future.

**CleanupDebugMessageSystem.cs**
```csharp
using Entitas;

public class CleanupDebugMessageSystem : ICleanupSystem
{
    readonly GameContext _context;
    readonly IGroup<GameEntity> _debugMessages;

    public CleanupDebugMessageSystem(Contexts contexts)
    {
        _context = contexts.game;
        _debugMessages = _context.GetGroup(GameMatcher.DebugMessage);
    }

    public void Cleanup()
    {
        // group.GetEntities() always gives us an up to date list
        foreach (var e in _debugMessages.GetEntities())
        {
            _context.DestroyEntity(e);
        }
    }
}
```

## Log Mouse Clicks

Lets extend our logging capabilities to log mouse clicks from the user. Here we will use an `IExecuteSystem` to listen for user clicks and create new DebugMessage entities. We can make use of Unity's Input class to grab user input and create new entities when inputs are received.

**LogMouseClickSystem.cs**
```csharp
using Entitas;
using UnityEngine;

public class LogMouseClickSystem : IExecuteSystem
{
    readonly GameContext _context;

    public LogMouseClickSystem(Contexts contexts)
    {
        _context = contexts.game;
    }

    public void Execute()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _context.CreateEntity().AddDebugMessage("Left Mouse Button Clicked");
        }

        if (Input.GetMouseButtonDown(1))
        {
            _context.CreateEntity().AddDebugMessage("Right Mouse Button Clicked");
        }
    }
}
```

# Incorporating new features

You've done most of the heavy lifting now, so incorporating new systems is easy. In larger projects you can separate out your systems into logically connected features and enforce execution order between them. Since our project is fairly simple, we will add our new systems to our existing feature `TutorialSystems`. 

```csharp
using Entitas;

public class TutorialSystems : Feature
{
    public TutorialSystems(Contexts contexts) : base ("Tutorial Systems")
    {
        Add(new HelloWorldSystem(contexts));
        Add(new LogMouseClickSystem(contexts)); // new system
        Add(new DebugMessageSystem(contexts));
        Add(new CleanupDebugMessageSystem(contexts)); // new system (we want this to run last)
    }
}
```

Now when you run your scene, you will notice that your hello world entity is no longer present, even though your message was displayed in the console. It was successfully deleted by our cleanup system. You'll also see your mouse clicks being logged to the console.

![More Success](http://i.imgur.com/pIZScoz.png)

*More success*

## Pooling in action

You might also notice that there is now 1 "reusable" entity listed on the game object in your hierarchy. This is entitas pooling the Entity for you to minimize garbage collection and memory allocation. Now try clicking your mouse. Notice your mouse clicks are being logged and their entities are also being cleaned up, and there is still only 1 reusable entity listed. This is because your mouse clicks were using the reusable one instead of creating a new one each time. If you try clicking both buttons together, two entities will be created in the same frame. Now you'll have two reusable entities. 

![Pooling](http://i.imgur.com/jQXz6DU.png)

*Pooling in Action*

# Next Steps

Now might be a good time to go back to the [MatchOne](https://github.com/sschmid/Match-One) example project. You should find it much easier to inspect the code in the project to determine how the effects you see on screen are achieved.








