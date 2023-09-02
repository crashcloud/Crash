﻿using Docker.DotNet;
using Docker.DotNet.Models;

namespace Crash.Handlers.Tests.Server
{
	public sealed class ServerUtils
	{
		public static async Task StartDockerContainer()
		{
			var client = new DockerClientConfiguration()
				.CreateClient();

			;

			IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
				                                           new ContainersListParameters { Limit = 10 });

			;
		}

		public void StopDockerContainer()
		{
		}

		public void VerifyDockerContainer()
		{
		}
	}
}
