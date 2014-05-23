﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Catrobat.IDE.Core.ExtensionMethods;
using Catrobat.IDE.Core.Models;
using Catrobat.IDE.Core.Models.Bricks;
using Catrobat.IDE.Core.Models.Formulas.Tree;
using Catrobat.IDE.Core.Models.Scripts;
using Catrobat.IDE.Core.Services.Storage;
using Catrobat.IDE.Core.Utilities.Helpers;
using System.IO;
using Catrobat.IDE.Tests.Extensions;

namespace Catrobat.IDE.Tests.Misc
{
    public class ProjectGeneratorReflection : ITestProjectGenerator
    {
        private Random _random;
        private readonly int? _seed;
        private readonly DateTime? _now;

        public ProjectGeneratorReflection()
        {
            _seed = null;
            _now = null;
        }

        public ProjectGeneratorReflection(int seed, DateTime now)
        {
            _seed = seed;
            _now = now;
        }

        // Bricks that must be tested manually
        private static readonly List<Type> ExcludedBricks = new List<Type>
        {
            typeof(ForeverBrick),
            typeof(EndForeverBrick),
            typeof(RepeatBrick),
            typeof(EndRepeatBrick),
            typeof(IfBrick),
            typeof(ElseBrick),
            typeof(EndIfBrick),
            typeof(EmptyDummyBrick)
        };

        private static readonly List<Type> ExcludedScripts = new List<Type>
        {
            typeof(EmptyDummyBrick)
        };

        public Project GenerateProject()
        {
            _random = _seed.HasValue ? new Random(_seed.Value) : new Random();
            var project = new Project
            {
                Name = "project1",
                Description = "",
                UploadHeader = new UploadHeader
                {
                    Uploaded = _now.HasValue ? _now.Value : DateTime.Now,
                    MediaLicense = "http://developer.catrobat.org/ccbysa_v3",
                    ProgramLicense = "http://developer.catrobat.org/agpl_v3",
                    RemixOf = "",
                    Tags = new ObservableCollection<string>(),
                    Url = "http://pocketcode.org/details/871",
                    UserId = ""
                }, 
                BroadcastMessages = new ObservableCollection<BroadcastMessage>()
            };

            var sprites = new ObservableCollection<Sprite>();
            project.Sprites = sprites;

            for (var i = 0; i < 2; i++)
            {
                sprites.Add(new Sprite
                {
                    Name = "Object" + i,
                    Costumes = new ObservableCollection<Costume>(),
                    Sounds = new ObservableCollection<Sound>(),
                });
            }

            for (var i = 0; i < 6; i++)
            {
                sprites[i % 2].Costumes = new ObservableCollection<Costume> {GenerateCostume(i, project)};
            }

            for (var i = 0; i < 6; i++)
            {
                sprites[i % 2].Sounds = new ObservableCollection<Sound> {GenerateSound(i, project)};
            }


            AddVariables(project);


            foreach (var sprite in sprites)
            {
                sprite.Scripts = new ObservableCollection<Script>();
                var scripts = ReflectionHelper.GetInstances<Script>(ExcludedScripts);
                foreach (var script in scripts)
                {
                    FillDummyValues(script, project, sprite);
                    sprite.Scripts.Add(script);

                    var bricks = ReflectionHelper.GetInstances<Brick>(ExcludedBricks);
                    foreach (var brick in bricks)
                    {
                        FillDummyValues(brick, project, sprite);
                        script.Bricks.Add(brick);
                    }
                    AddLoopBricks(script.Bricks);
                    AddIfLogicBricks(script.Bricks);
                }
            }

            return project;
        }

        private void FillDummyValues(object o, Project project, Sprite sprite)
        {
            var type = o.GetType();

            foreach (var property in type.GetProperties())
            {
                if (property.CanWrite)
                {
                    var value = CreateDummyValue(property.PropertyType, project, sprite);
                    property.SetValue(o, value, null);
                }
            }
        }

        private object CreateDummyValue(Type type, Project project, Sprite sprite)
        {
            if (type == typeof(bool))
            {
                return true;
            }

            if (type == typeof(int))
            {
                return 42;
            }

            if (type == typeof(string))
            {
                return "SomeString";
            }

            if (type == typeof(double))
            {
                return 42.42;
            }

            if (type == typeof(FormulaTree))
            {
                return GenerateFormula(project, sprite);
            }

            if (type == typeof(Sprite))
            {
                return _random.Next(project.Sprites);
            }

            if (type == typeof(Costume))
            {
                var costumes = sprite.Costumes;
                return costumes[_random.Next(0, costumes.Count - 1)];
            }

            if (type == typeof(Sound))
            {
                var sounds = sprite.Sounds;
                return sounds[_random.Next(0, sounds.Count - 1)];
            }

            if (type == typeof(Variable))
            {
                return _random.NextBool()
                    ? (Variable)_random.Next(sprite.LocalVariables)
                    : _random.Next(project.GlobalVariables);
            }

            if (type == typeof(IfBrick))
            {
                return sprite.Scripts.SelectMany(script => script.Bricks).OfType<IfBrick>().FirstOrDefault();
            }
            if (type == typeof(ElseBrick))
            {
                return sprite.Scripts.SelectMany(script => script.Bricks).OfType<ElseBrick>().FirstOrDefault(); 
            }
            if (type == typeof(EndIfBrick))
            {
                return sprite.Scripts.SelectMany(script => script.Bricks).OfType<EndIfBrick>().FirstOrDefault();
            }

            if (type == typeof (ObservableCollection<Script>))
                return new ObservableCollection<Script>();

            if (type == typeof(ObservableCollection<Brick>))
                return new ObservableCollection<Brick>();

            return null;
        }

        private FormulaTree GenerateFormula(Project project, Sprite sprite)
        {
            return FormulaTreeFactory.CreateSubtractNode(
                    leftChild: FormulaTreeFactory.CreateSinNode(FormulaTreeFactory.CreateNumberNode(6)), 
                    rightChild: FormulaTreeFactory.CreateLocalVariableNode(VariableHelper.GetLocalVariableList(project, sprite).FirstOrDefault()));
        }

        private Costume GenerateCostume(int index, Project project)
        {
            var costume = new Costume
             {
                 FileName = "FileName" + index,
                 Name = "Look" + index,
             };

            var absoluteFileName = Path.Combine(project.BasePath, Project.ImagesPath, costume.FileName);

            using (var storage = StorageSystem.GetStorage())
            {
                var fileStream = storage.OpenFile(absoluteFileName, StorageFileMode.Create, StorageFileAccess.Write);
                fileStream.Close();
            }

            return costume;
        }

        private Sound GenerateSound(int index, Project project)
        {
            var sound = new Sound
                {
                    FileName = "FileName" + index,
                    Name = "Sound" + index,
                };

            var absoluteFileName = Path.Combine(project.BasePath, Project.SoundsPath, sound.FileName);

            using (var storage = StorageSystem.GetStorage())
            {
                var fileStream = storage.OpenFile(absoluteFileName, StorageFileMode.Create, StorageFileAccess.Write);
                fileStream.Close();
            }

            return sound;
        }

        private void AddVariables(Project project)
        {
            foreach (var sprite in project.Sprites)
            {
                sprite.LocalVariables = new ObservableCollection<LocalVariable>
                {
                    new LocalVariable
                    {
                        Name = "LocalTestVariable"
                    }
                };
            }

            project.GlobalVariables = new ObservableCollection<GlobalVariable>();
            for (var i = 0; i < 3; i++)
            {
                project.GlobalVariables.Add(new GlobalVariable
                {
                    Name = "GlobalTestVariable" + i
                });
            }
        }

        private void AddLoopBricks(ObservableCollection<Brick> bricks)
        {
            var foreverBrick = new ForeverBrick();
            bricks.Add(foreverBrick);

            var repeatBrick = new RepeatBrick
            {
                Count = FormulaTreeFactory.CreateNumberNode(2)
            };
            bricks.Add(repeatBrick);

            var loopEndBrickForever = new EndForeverBrick();
            bricks.Add(loopEndBrickForever);

            var loopEndBrickRepeat = new EndRepeatBrick();
            bricks.Add(loopEndBrickRepeat);

            foreverBrick.End = loopEndBrickForever;
            repeatBrick.End = loopEndBrickRepeat;
            loopEndBrickForever.Begin = foreverBrick;
            loopEndBrickRepeat.Begin = repeatBrick;
        }

        private void AddIfLogicBricks(ObservableCollection<Brick> bricks)
        {
            var ifLogicBeginBrick = new IfBrick
            {
                Condition = FormulaTreeFactory.CreateTrueNode()
            };
            bricks.Add(ifLogicBeginBrick);

            var ifLogicElseBrick = new ElseBrick();
            bricks.Add(ifLogicElseBrick);

            var ifLogicEndBrick = new EndIfBrick();
            bricks.Add(ifLogicEndBrick);


            ifLogicBeginBrick.Else = ifLogicElseBrick;
            ifLogicBeginBrick.End = ifLogicEndBrick;

            ifLogicElseBrick.Begin = ifLogicBeginBrick;
            ifLogicElseBrick.End = ifLogicEndBrick;

            ifLogicEndBrick.Begin = ifLogicBeginBrick;
            ifLogicEndBrick.Else = ifLogicElseBrick;
        }

    }
}
