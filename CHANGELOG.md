# Changelog
The changelog lists changes to the application. Each entry includes the date, new or removed features, identified issues, and bug fixes.

Dates are in `YYYY/MM/DD`. Versions are in the form `X.Y.Z`, where `X` is a significant development stage, `Y` is a set of features, and `Z` is a minor change or hotfix. 

---

### Version 0.6.0 - MVVM & Modularisation Pit Stop
<details>
<summary>
  <b>Date:</b> 2026/11/27
</summary>

**Features:**
* Implemented MVVM architecture for scalability moving forward:
  * Introduced new ViewModel classes (among other MVVM related things)
  * Modularised "item listing" (Projects, Resources, Subslots)
  * Separated complex sub-view components (namely Resource Assignment and Task Listing)
  * Slightly adjusted data models to fit the architecture
* Adjusted some page designs and layouts
* Cleaned up and modularised other code and methods

**Bug Fixes:** No new bugs discovered or existing bugs fixed.

**NOTE:** implementing the architecture took a lot of refactoring. There may be many inconsistencies in the code for this version.

</details>

---

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
