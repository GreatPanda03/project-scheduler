# Changelog
The changelog lists changes to the application. Each entry includes the date, new or removed features, identified issues, and bug fixes.

Dates are in `YYYY/MM/DD`. Versions are in the form `X.Y.Z`, where `X` is a significant development stage, `Y` is a set of features, and `Z` is a minor change or hotfix. 

---

### Version 0.6.0 - Modularisation & Code Cleanup
<details>
<summary>
  <b>Date:</b> 2026/11/16
</summary>

**Features:**

* TODO: upload the official v0.5.0 .zip to ChatGPT


* TODO: make it so that the user can create multiple pre-work checklists

* firstly - rename "Pre-work Checklist" to simply be "Checklist"
* this is a smuch simpler name to deal with, especially in designs and descriptions
* semantically, "checklist" is also distinct from "task" since the former is repeatable
* the latter can also technically be repeatable... but that'll be a future feature

* i'll also have to rename "PreWorkItem" to "ChecklistItem"
* there'll probably also need to be a "ChecklistsItem" object (for multiple ChecklistItems)
* consult ChatGPT and see what it thinks (it'll probably want to do something similar to 
    TaskItem and TaskListEntry)

* these should be assignable to a project
* the user should also be able to assign ONE checklist to the home page
* this will allow the user to complete a set of tasks BEFORE starting a project
* this is different to, say, a Preparation subslot at the start of a project
   * a subslot like this doesn't have tasks of its own - its just a time period for
       some kind of concept of work (in this case, Preparing for a project)
* ... but checklists should always be done at the start of projects anyway

* the idea mainly sprung from wanting to make each page consistent...
* but it actually has a value
* any project which isn't assigned a checklist simply won't have one at the start
* this means the user isn't forced to assign a checklist to projects, they can choose to

* HOWEVER - if a checklist or project is deleted, then the associations should be removed


* TODO: "modularise" the similarities between all list-like pages
* this'll be difficult in some ways because the list and display functionalities are all the same
* ... but the entry contents and the way entries are displayed are different
* SO: consult ChatGPT with the kinds of similarities and differences these structures have
*   then ask it what the best way would be to modularise the code

* this applies to LOTS of other sections - i'll have to make a full list of where i see these
* the Projects page in particular is PACKED with code...
* i can only imagine what the Schedules page will be like (3000 lines at this rate)

</details>


### Version 0.5.0 - Tasks & Project Managament
<details>
<summary>
  <b>Date:</b> 2026/11/15
</summary>
  
**Features:**
* Implemented the Projects page:
  * Projects can now be created, edited, and deleted
  * Projects are a collection of Tasks (see below) and Resources to work towards a goal
  * Projects have also have a Name, Description, Priority, and additional Notes
  * Deleting a Project will delete any Tasks associated with it
* Implemented Tasks:
  * Tasks can be created, edited, and deleted
  * Tasks define actions to complete for a Project
  * Tasks have a Name and can be marked as complete
  * Tasks can have Subtasks, which in turn can have Subtasks (ie. task breakdown)
* Updated Resources:
  * Deleting a Resource removes it from any Project which uses it
* Updated screen designs:
  * Slightly adjusted the designs for the Resources and Subslots pages

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

</details>

---

### Version 0.4.0 - Subslot Management
<details>
<summary>
  <b>Date:</b> 2026/11/14
</summary>

**Features:**
* Implemented the Subslots page:
  * Subslots can now be created, edited, and deleted
  * Subslots can be given a Name, Description, and additional Rules
  * Currently, the only rule is "Lock Resources", which disables access to project resources

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

</details>

---

### Version 0.3.0 - Resource Management
<details>
<summary>
  <b>Date:</b> 2026/11/14
</summary>

**Features:**
* Implemented the Resources page:
  * Resources can now be created, edited, and deleted
  * Resources can be given a Name, Description, Type, and Directory
  * There are 3 resource types: Documents, Apps, and Websites
  * Web resources are opened in the user's default browser

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

</details>

---

### Version 0.2.0 - Pre-work Schedule Customisation
<details>
<summary>
  <b>Date:</b> 2026/11/13
</summary>

**Features:**
* Updated the Home Page:
  * Added a button to start a schedule
  * App sections & schedule starting are locked while any Pre-work Checklist is incomplete
* Updated the Pre-work Checklist Page:
  * Custom pre-work checklist items can be created, edited, and deleted
  * Existing pre-work checklist items are listed on the page, as on the Home Page (cannot be marked as complete)
  * Pre-work Checklist items save are now saved to the local storage

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

</details>

---

### Version 0.1.0 - Navigation & Pre-work Checklist
<details>
<summary>
  <b>Date:</b> 2026/11/10
</summary>

**Features:**
* Created the base of the application from a WinUI 3 template
* Implemented the Navigation panel with all sections included
* Implemented a hard-coded, completable prework checklist on the home page

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

</details>
