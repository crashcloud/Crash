<a name="readme-top"></a>

<!-- PROJECT SHIELDS -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<!-- PROJECT LOGO -->

<div align="center">
  <a href="https://github.com/crashcloud/Crash">
    <img src="art\crash-logo.jpg" alt="Logo">
  </a>

  <p align="center">
    A multi-user collaborative environment for Rhino
    <br />
    <a href="http://crsh.cloud">User Guide</A>
    ·
    <a href="https://github.com/crashcloud/Crash/issues">Report Bug</a>
    ·
    <a href="https://github.com/crashcloud/Crash/issues">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#baby-poweruser-getting-started">Poweruser Getting Started</a>
    </li>
    <li>
      <a href="#man_technologist-woman_technologist-developer-getting-started">Developer Getting Started</a>
    </li>
    <li><a href="#workflow-overview">Workflow Overview</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->

## About The Project

This project has been completed as part of the TT AEC Hackathon 2022 - New York. This plugin/application allows users to
collaborate on a single central Rhino model. The Team Members for this awesome project are (in alphabetical order):

* [Callum Sykes](https://www.linkedin.com/in/callumsykes/)
* [Curtis Wensley](https://www.linkedin.com/in/cwensley/)
* [Erika Santos](https://www.linkedin.com/in/erikasantosr/)
* [Lukas Fuhrimann](https://www.linkedin.com/in/lfuhrimann/)
* [Morteza Karimi](https://github.com/karimi)
* [Moustafa El-Sawy](https://www.linkedin.com/in/moustafakelsawy/)
* [Russell Feathers](https://www.linkedin.com/in/russell-feathers/)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
* [SignalR](https://learn.microsoft.com/en-us/aspnet/signalr/overview/getting-started/introduction-to-signalr)
* [SQLite](https://www.sqlite.org/index.html)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- POWERUSER GETTING STARTED -->

## :baby: Poweruser Getting Started

Thanks for checking out CRASH! Please follow the steps below to get started in no time! Please make sure you have all
the <a href="#prerequisites">Prerequisites</a> to have a smooth and fun experience!

### Prerequisites

You will need the following libraries and/or software installed before getting to the fun!

* [Rhino 7.21+](https://www.rhino3d.com/download/)

### Installing CRASH from YAK

1. Launch Rhino 7 (or 8)
2. Type in PackageManager or go to Tools --> Package Manager
3. Search for Crash and press Install.
4. Close and Re-launch Rhino 7 (or 8).

### Using Crash

To host a new shared model:

1. Type `StartSharedModel` command in Rhino.
2. Enter your name when prompted.
3. Specify an open port on your machine to run the server
4. Others can join the session using url `<your_ip_address>:<port>`

![Alt Text](https://media.giphy.com/media/oNuY0wsiDV5XFmYuNw/giphy.gif)

To Join a shared model:

1. Type `OpenSharedModel` command in Rhino.
2. Enter your name when prompted.
3. Enter the server URL from step 4 above.

You're now connected in a collaborative session. To commit your changes to the central model use the `Release` command.

<!-- DEVELOPER GETTING STARTED -->

## :man_technologist: :woman_technologist: Developer Getting Started

Thanks again for checking out CRASH! Please follow the steps below to get started and diving into the code in no time!
Please sure sure you have all the <a href="#prerequisites-1">Prerequisites</a> to have a smooth, unbuggy and fun
experience!

### Prerequisites

You will need the following libraries and/or software installed before getting to the fun!

* [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
* [.NET Core 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* [Rhino 7.21+](https://www.rhino3d.com/download/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

### Prerequisites (MacOS)

You can also build and debug on MacOS using VS Code!

* [Visual Studio Code](https://code.visualstudio.com/)
* [Rhino 8 WIP](https://www.rhino3d.com/download/rhino/wip) is required on ARM machines.

### Prerequisites (Either)

Crash wil automatically download crash.server, but you can also clone and run your own in debug mode.

* [crash.server](https://github.com/crashcloud/crash.server)

### Getting Source

Clone the repo

   ```sh
   git clone https://github.com/crashcloud/crash.git
   ```

### Building

#### Windows

Open Crash repository in Visual Studio:

1. Set Crash as startup project.
2. Build solution.
3. Drag and drop `Crash\Crash\bin\**\**\Crash.rhp` into an open Rhino window.
4. Re-open Rhino.
5. Happy debugging.

#### MacOS

Open Crash repository in VS Code run build tasks `⇧⌘B` in this order:

1. `buid-plugin`
2. `build-server`
3. `publish-server`
   From `Run and Debug` tab run `Run Rhino 8 WIP`

Rhino will launch in debug mode.

<!-- CONTRIBUTING -->

## Contributing

See the [open issues](https://github.com/crashcloud/Crash/issues) for a full list of proposed features (and known
issues).

[Please see contribution guide](CONTRIBUTING.md)

<!-- LICENSE -->

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<!-- ACKNOWLEDGMENTS -->

## Acknowledgments

Big thanks to AEC Tech 2022 for arranging this event! Also we would like to thank McNeel for all their awesome work!
This project has been a great collaboration of several great minds. Please check out other hackathon projects and future
hackathon events hosted by [AECTech](https://www.aectech.us/). Original repo can be found
at : [crashcloud/crash](https://github.com/crashcloud/crash)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/crashcloud/Crash.svg?style=for-the-badge

[contributors-url]: https://github.com/crashcloud/Crash/graphs/contributors

[forks-shield]: https://img.shields.io/github/forks/crashcloud/Crash.svg?style=for-the-badge

[forks-url]: https://github.com/crashcloud/Crash/network/members

[stars-shield]: https://img.shields.io/github/stars/crashcloud/Crash.svg?style=for-the-badge

[stars-url]: https://github.com/crashcloud/Crash/stargazers

[issues-shield]: https://img.shields.io/github/issues/crashcloud/Crash.svg?style=for-the-badge

[issues-url]: https://github.com/crashcloud/Crash/issues

[license-shield]: https://img.shields.io/github/license/crashcloud/Crash.svg?style=for-the-badge

[license-url]: https://github.com/crashcloud/Crash/blob/master/LICENSE.txt

[product-screenshot]: images/screenshot.png
