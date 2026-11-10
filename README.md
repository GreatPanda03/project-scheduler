# "Project Scheduler" - focus on what matters, when it matters
A personalised WinUI 3 project to help me complete projects through scheduling and app blocking.

***Disclaimer:** GenAI was almost entirely used to write the WinUI 3 code for the project. All other artefacts are human-made.*

# Contents
1. [Problem Definition](#problem-definition)
2. [Existing Services](#existing-services)
3. [Development Process](#development-process)
4. [Final Application](#final-application)
5. [Project Artefacts](#project-artefacts)
6. [App Installation](#app-installation)

## Problem Definition
I have a strong tendency to procrastinate! This means I don't always do what matters when I technically should be doing it. This isn't limited to work, and includes assignments, programming, and even games. Collectively, I call these my **"projects"**.

[Tim Urban's TedTalk](https://www.ted.com/talks/tim_urban_inside_the_mind_of_a_master_procrastinator) captures this very well. Projects are pushed back until the moment something scary comes along, like a deadline. This is when I'll kick into gear and get it done. However, when there isn't a deadline, this becomes so bad that I often don't complete or even start these projects.

***High friction*** is the main factor which make approaching projects difficult. "Complete my computer science assignment" is a hefty feat, and seeing such a large task makes me less keen to do it than, for example, play a game, which is far more ***predictable***. To combat this, I typically decompose them into smaller predictable tasks, like "Do some research" or "Write the introduction", which are decomposed in turn. The idea is to make tasks so easy to do that they're unavoidable.

Similarly, scheduling and planning is easy, but ***execution and retention*** is much harder. Since my work and leisure are both on my laptop, it's easy to bounce from work to leisure and stay on leisure for the whole day without moving back to work. I have tried curbing it, but "willpower" doesn't cut it, and I feel something which takes the possibility away would be better. Sticking to habits is also very difficult, and it isn't rare to try something new for a few days only to forget about it when something comes up.

## Existing Services
As all projects are managed with my laptop, it makes sense to leverage ***technology*** to try to solve my problem. Solutions do exist to help manage both time and app use, such as Motion, ScreenZen, Morgen, TimeBloc, Opal, and many more. However, they either: 
* Require a subscription (the main blocker)
* Are too overstimulating or have many bells and whistles
* Only allow time or app blocking, not both
* Don't offer enough app limiting control or can easily be bypassed
* Don't offer enough task listing capabilities
* Aren't integrated with my laptop as a whole in a way that's persistent

This leaves me with a mixed bag of solutions to choose from. While it's feasible to combine them, it wouldn't be sustainable as the routine of using them would be hard to maintain. I would also prefer a tool which is personalised to my specific pitfalls and habits.

## Development Process
The app was developed with ***WinUI 3*** as it provides Windows integration for app blocking, scheduling, and resource management. It also has plenty of official documentation and is similar to web development (XAML & CS and used like HTML & JavaScript).

I had originally planned to learn WinUI 3 myself. To start with, I created a plan for the app by capturing my requirements and iteratively refining them against screen designs and process flows. This was all done over a week, after which I hadn't worked on the project for 2 months! So, I decided to reduce the friction and use this opportunity to fully code an app with ChatGPT (GPT-6 as of 2026/11/10), which I'd never done before.

I initially used ChatGPT to create the application in bulk from my [Planning Diagram](./Planning%20Diagram.drawio). While it perfectly understood my requirements and designs, the app itself was a mess. I decided to do it feature by feature instead, which was a much smoother experience. All logs are available at the following link for references: https://chatgpt.com/g/g-p-6af3139df150819198bb4491a0fa360e-scheduling-app/project.

## Final Application
The ***"Project Scheduler"*** is a personalised app which addresses the gaps identified above and is tailored to my workflow. You can use it to:
* Start the day with a set of actions
* Create projects with assigned resources and prioritised tasks
* Split tasks into subtasks with assigned resources
* Organise projects into a schedule for the day
* Blacklist apps, websites, or documents from ongoing project slots
* Tailor sub-slots within a project for finer scheduling

## Project Artefacts
The repository includes other artefacts to help understand the context of the application, what has been implemented, and where improvements could be made:
* [**Planning Diagram:**](broken) a `.drawio` diagram with initial requirements gathering and planning, including screen designs and process flows.
* [**Requirements Backlog:**](broken) an Azure DevOps backlog which tracks the projects requirements with their descriptions, acceptance criteria, statuses, and more.
* [**Changelog:**](broken) a list of all features, bug fixes, and other changes added to the project code.

## App Installation
*(work in progress)*
