using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Fusion;
using NUnit.Framework;
using UnityEngine;

namespace PlowParty.Infrastructure.Tests
{
    public sealed class NetworkWeavingTests
    {
        private const string ProjectAssemblyPrefix = "PlowParty.";
        private const string ConfigPath = "Photon/Fusion/Resources/NetworkProjectConfig.fusion";

        [Test]
        public void EveryNetworkAssembly_IsListedInAssembliesToWeave()
        {
            var woven = ReadAssembliesToWeave();

            var missing = NetworkAssemblies().Select(assembly => assembly.GetName().Name).Where(name => !woven.Contains(name));

            Assert.That(missing, Is.Empty);
        }

        [Test]
        public void EveryAssemblyDeclaringRpcs_AllowsUnsafeCode()
        {
            var verified = NetworkTypes().Where(type => RpcsOf(type).Any()).Select(type => type.Assembly).Distinct()
                .Where(assembly => !assembly.ManifestModule.IsDefined(typeof(UnverifiableCodeAttribute), false))
                .Select(assembly => assembly.GetName().Name);

            Assert.That(verified, Is.Empty);
        }

        [Test]
        public void EveryRpc_ReachesFusionWithoutAccessViolation()
        {
            var failures = new List<string>();
            foreach (var type in NetworkTypes().Where(type => !type.IsAbstract && typeof(NetworkBehaviour).IsAssignableFrom(type)))
            {
                foreach (var rpc in RpcsOf(type))
                {
                    var violation = AccessViolationOf(type, rpc);
                    if (violation != null)
                    {
                        failures.Add($"{type.FullName}.{rpc.Name}: {violation.Message}");
                    }
                }
            }

            Assert.That(failures, Is.Empty);
        }

        private static MemberAccessException AccessViolationOf(Type type, MethodInfo rpc)
        {
            var host = new GameObject(type.Name);
            try
            {
                var target = rpc.IsStatic ? null : host.AddComponent(type);
                rpc.Invoke(target, rpc.GetParameters().Select(DefaultOf).ToArray());
                return null;
            }
            catch (TargetInvocationException exception)
            {
                return exception.InnerException as MemberAccessException;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static object DefaultOf(ParameterInfo parameter)
        {
            return parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
        }

        private static IEnumerable<Assembly> NetworkAssemblies()
        {
            return NetworkTypes().Select(type => type.Assembly).Distinct();
        }

        private static IEnumerable<Type> NetworkTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith(ProjectAssemblyPrefix, StringComparison.Ordinal))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(IsNetworkType);
        }

        private static bool IsNetworkType(Type type)
        {
            return typeof(NetworkBehaviour).IsAssignableFrom(type) || typeof(INetworkInput).IsAssignableFrom(type);
        }

        private static IEnumerable<MethodInfo> RpcsOf(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetMethods(flags).Where(method => method.IsDefined(typeof(RpcAttribute), false));
        }

        private static HashSet<string> ReadAssembliesToWeave()
        {
            var json = File.ReadAllText(Path.Combine(Application.dataPath, ConfigPath));
            return new HashSet<string>(JsonUtility.FromJson<WeaveList>(json).AssembliesToWeave);
        }

        [Serializable]
        private sealed class WeaveList
        {
            public string[] AssembliesToWeave = Array.Empty<string>();
        }
    }
}
