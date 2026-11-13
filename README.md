# "Project Scheduler" - focus on what matters, when it matters
A personalised WinUI 3 project to help me complete projects through scheduling and app blocking.

***Disclaimer:** GenAI was almost entirely used to write the WinUI 3 code for the project. All other artefacts are human-made.*

## Contents
1. [Problem Definition](#problem-definition)
2. [Existing Services](#existing-services)
3. [Development Process](#development-process)
4. [Final Application](#final-application)
5. [Project Artefacts](#project-artefacts)
6. [App Installation](#app-installation)

## Problem Definition
I have a strong tendency to procrastinate, which means I don't always do what matters when I should! This includes work, assignments, programming, and even games I want to play. Collectively, I call these my **"projects"**.

[Tim Urban's TedTalk](https://www.ted.com/talks/tim_urban_inside_the_mind_of_a_master_procrastinator) captures this very well. Projects are pushed back until high pressure moments, like a deadline, which is when I complete the work. However, it's difficult to hold myself accountable when there isn't one.

Primarily, ***high friction*** makes approaching projects difficult. Seeing a large task like "Complete my computer science assignment" is more daunting than, for example, playing a game, which is far more ***predictable***.

To combat this, I typically decompose them into smaller predictable tasks, like "Do some research" or "Write the introduction", which are decomposed in turn. The idea is to make tasks so easy to do that they're unavoidable, and has worked well for me.

Decomposing tasks and orchestrating schedules is straightforward, but ***execution and retention*** is another issue. Having both work and leisure on my laptop makes it easy to avoid work by doing something else. Keeping good habits is also difficult since I normally forget them after a few days without persistent reminders.

In this project, my goal to find a solution which helps me dedicate time to my projects while incorporating my existing techniques and addressing my challenges.

## Existing Services
As all projects are managed with my laptop, it makes sense to leverage ***technology*** to help me.

Solutions do exist to help manage both time and app use, such as Motion, ScreenZen, Morgen, TimeBloc, Opal, and many more. However, they either: 
* Require a subscription (the main blocker)
* Are too overstimulating or have many bells and whistles
* Only allow time or app blocking, not both
* Don't offer enough app limiting control or can easily be bypassed
* Don't offer enough task listing capabilities
* Aren't integrated with my laptop as a whole in a way that's persistent

This leaves several possibilities. Combining them is feasible but not sustainable I would have to maintain a manual routine, which is already difficult. Most tools also miss elements others have that would be great together.

## Development Process
The app was developed with ***WinUI 3*** as it provides Windows integration to facilitate app blocking, scheduling, and resource management. It also has up-to-date documentation and is similar to web development (XAML & C# are used like HTML & JavaScript).

I captured my application requirements with a planning diagram and iteratively refined them against screen designs. Then, I formally wrote these in an Azure DevOps backlog. I used this opportunity to fully develop the app with ChatGPT (GPT-6 as of 2026/11/10), which I'd never done before, to passively learn WinUI 3 and accelerate the process.

I initially prompted ChatGPT to create the application in bulk from my planning diagram. Although it understood my requirements and designs, the app itself had malformed UI, lacking functionality, and many bugs. I decided to develop each feature step-by-step instead, which was a much smoother experience.

All logs are available at the following link for references: https://chatgpt.com/g/g-p-6af3139df150819198bb4491a0fa360e-scheduling-app/project.

## Final Application
The ***"Project Scheduler"*** is a personalised app which addresses the gaps identified above and is tailored to my workflow. You can use it to:
* Start the day with a set of actions
* Create projects with assigned resources and prioritised tasks
* Split tasks into subtasks with assigned resources
* Organise projects into a schedule for the day
* Whitelist/blacklist apps, websites, or documents from projects
* Tailor sub-slots within a project for finer workflows

## Project Artefacts
The repository includes artefacts to show what has been implemented and where improvements could be made:
* [**Project Scheduler:**](./Project%20Scheduler) the source code for the project.
* [**Planning Diagram:**](./Requirements/Planning%20Diagram%20(2026-11-10).drawio) a `.drawio` diagram with initial requirements gathering and planning, including screen designs and process flows.
* [**Initial Requirements:**](./Requirements/Initial%20Requirements%20(v0.1.0).pdf) an Azure DevOps backlog which tracks the projects requirements with their descriptions, acceptance criteria, statuses, and more.
* [**Backlog Snapshots:**](./Backlog%20Snapshots) a snapshot of user story states per version.
* [**Changelog:**](./CHANGELOG.md) a list of all features, bug fixes, and other changes added to the project code.


## App Installation
*(work in progress)*
