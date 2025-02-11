Rock McKinley 4.3

+ Fixed issue with check-in "randomly" not allowing check-ins
+ Fixed issue with registration saving a blank person/registrant if user navigated away from registration and then used browser navigation to return before submitting the registration.
+ Fixed Registration Entry so that it does not create person notes until registration and payment was successful.
+ Fixed Tag Report so that after sorting it deletes the correct item (Fixes #1398).
+ Fixed location details not saving attribute values.
+ Fixed issue with security when changing if a group is a security role or not (Fixes #1399).
+ Fix issue with Group Requirements when the Expire In Days setting has not been set (Fixes #1389).
+ Fixed error in the People REST endpoint (Fixes #1388).
+ Updated Check-in to correctly cache device configuration data so that it is not specific to a particular point in time.
+ Updated Lava debug display to only display each object type/id once, and limit iterations to first two items (Fixes #1365).
+ Changed z-index of the admin toolbar to keep it from being hidden when editing page zones. (Fixes #1397)
+ Changed the ConnectionRequestDetail block to allow a request to be Connected even if there is no placement group. In the future this can be controlled via a new 'RequiresPlacementGroupToConnect' flag on the opportunity type.
+ Fixed Communication Entry block so that the Subject is cleared if a selected template does not specify a subject (Fixes #1393).
+ Fixed IE double-tap zoom issue in check-in themes
+ Fixed issue where creating a new communication from a dataview caused the block to timeout before the configured database timeout occurred. (Fixes #909).
+ Fixed issue when trying to add group attendance and the location filter is set to a parent location (i.e. campus/building) vs. an actual room location.
+ Updated the Group Attendance Detail block so that if adding attendance for a particular schedule, the attendance record gets created with the correct start time.
+ Connection Search block now has access to the complete opportunity model including the opportunity attributes. Note, there is a small change in the Lava for this update. If you use a custom theme you'll want to grab a new copy of 'OpportunitySearch.lava' as you should use the property of 'PublicName' instead of 'Name'.

Rock McKinley 4.2

+ Updated Registration Entry so that discount code is still applied correctly when user returns to make a payment on existing registration.
+ Updated Registration Entry to correctly delete a registration if it was just created but an error occurred during save of registrants or attributes.
+ Fixed exception that would occur when copying a registration template that included an additional form(s) (in addition to the default form) and that form contained a person or group attribute (Fixes #1356).
+ Fixed registration issue that would result in registered person getting added to target group even if registration failed due to payment issue.
+ Fixed issue with group attendance not recording attendance correctly and continuing to send reminders.
+ Fixed issue where a person's tags would not always survive a merge.
+ Fixed performance issue with the Giving Amount dataview filter due to how it generated its query.
+ Fixed so that Connection Opportunities in the Connection Request transfer window are now ordered.
+ Added real person photos to the group member list block. This is not only an enhancement but also fixes what appears to be a UI bug when no photo exists.
+ Updated the Workflow Type Detail block so that it does not time out when trying to remove an activity type or action from an existing workflow type that has a significant number of workflows already created.
+ Updated the Rock Shop configuration to work with usernames and/or passwords that may have special characters.
+ Updated the Send Email and the Send System Email workflow actions to allow Email attribute field types for the from and to addresses (in addition to text or person field types)
+ Removed unnecesary clearing of authorization cache whenever a group member was added/edited/removed from a security group.
+ Updated the Group Type Detail so that if a group member attribute is removed, it is also removed from any registration template that was configured to use that attribute.
+ Updated the Twilio webhook so that if it gets an IOException when writing to log file that it will wait and try again a few times before causing an error.
+ Fixed issues with numeric attribute field comparisons on data views (Fixes #1377).
+ Fixed issue with Group and Role picker not saving the selected role (Fixes #1380).
+ Updated the Transaction Entry and Registration Entry blocks to validate new passwords against configured pattern when user is creating a new login in order to save their payment account information (Fixes #1371).
+ Fixed bug where event name not showing in page title (Fixes #1373)
+ Fixed issue with attendance block not saving the location or schedule for a new attendance being entered.
+ Fixed issue in Statement Generator where some transactions might not show up if it is split into multiple accounts and at least one of those accounts wasn't included in the filter.

Rock McKinley 4.1

+ Updated communication entry block so that binary file attachments are not stored as temporary files (and possibly deleted up before an email is sent in the future).
+ Fixed issue with payment details getting cleared when a scheduled transaction is updated.
+ Fixed issue with a reporting lava column being blank when exporting to excel (fixes #1349).
+ Updated the cache timeout on check-in location attendance counts. Was previously changed from 1 min to an hour, but an hour is too long to accurately reflect current attendance in a location.
+ Fixed the Defined Value field type filter when the option to display multiple values is enabled.
+ Updated REST methods so they do not pass their rockcontext object (without proxy creation enabled) to the cache methods that rely on being able to use navigation properties to load child collection properties.
+ Updated Registration Entry so that it does not block existing registrations from editing an existing registration when the instance is full.
+ Fixed exception that would occur when attempting to add a new registrant who already belonged to the group associated with the registration.
+ Fixed exception that would occur if a fee was deleted from a registrant template that had existing registrants already using the fee. It will now remove the fee from the registrants.
+ Fixed exception that would occur when unselecting a fee from a registrant.
+ Fixed baptism badge html markup.
+ Fixed an issue with Connection signup where new records were being created even when logged in.
+ Updated how registration person field values are serialized so that objects can be unserialized correctly.
+ Fixed security issue on metric detail that allowed non-authorized users to edit/delete a metric (Fixes #1338).
+ Fixed issue with undelivered emails appearing in a person's communication history list (Fixes #1344).
+ Fixed Pledge Analytics to display business names (Fixes #1335).
+ Fixed issue with date picker not working correctly on New Family block (Fixes #1333).
+ Fixed the workflow SMS Send action to support using 'Person' merge field when recipient is a person, group, or security role attribute (Fixes #1309).
+ Fixed issue with transaction matching when masked account number was blank.
+ Fixed an issue preventing Email, Campus and Fee columns from exporting to Excel.

Rock McKinley 4.0

+ Added support for processing refunds.
+ Added Twitter Authentication Service.
+ Added Alternate Placement feature to groups.
+ Added following to Groups.
+ Added Connection Management feature to help with the process of finding and placing volunteers into teams.
+ Added a new core Group Requirement Type and Data View for Background Checks
+ Added new Attendance Analysis attendee grid that shows detailed information on a person's attendance, which can be filtered in some handy ways
+ Added Google Login provider
+ Refactored giving to allow multiple financial gateways to be configured using the same payment processor with different settings.
+ Added setting to display the middle name of a person on the bio block. By default the setting is set to false.
+ Changed default security on new sites so that they do not require user to login.
+ Updated content channel items to correctly sort items when sorting on numeric and/or date attribute values.
+ Updated the attendance list/detail blocks to allow deleting and changing the date of any occurrence that is not tied to a specific schedule
+ Added the ability to remove a person from a family (vs. moving them to a new family) if they are already in another family.
+ Updated the category tree view (used by data views, reports, etc.) to allow a user to edit items if they have security for a specific category rather than requiring edit rights to the block or page.
+ Updated the person merge to remove any duplicate previous names after merging records.
+ Added the option for administrators to disable verification services for a particular location on the person profile page (Fixes #1286).
+ Added additional Lava to output a phone number given a person input.
+ Added Lava filters to get the parents or children of a given person.
+ Updated the Content Channel View block to support filtering item by a route parameter as well as a query string parameter.
+ Updated the ContentChannelView blocks to only do query string parameter filtering if option is set on block (Fixes #1267).
+ Added option to Group Type to show each group member's Connection Status in the group member list.
+ Added option to the header of the TransactionList block to show images or summary. The option can be made visible in block options.
+ Added 'Inactivate Child Groups' feature to Group.
+ Added REST EndPoint GetGraduationYear for getting a graduation year from a grade offset.
+ Added Inactive Reason Note to EditPerson page.
+ Fixed person search for people with two first names (e.g. Mary Ann Smith).
+ Added options for sending a contribution receipt whenever someone gives a onetime gift online, or when a recurring transaction is downloaded for their scheduled transaction.
+ Added option to the group list to display the full path to group.
+ Added new Last Note data select for Person reports.
+ Added "Move Group Member" feature to Group Member Detail.
+ Added new File Manager block that can be used to manage and upload files.
+ Added Campus Filter to Group Member List.
+ Updated the display of admin bars to only show if current user has administrative rights to the page or a block on page, or edit rights to a block that has custom settings (HTML Detail, Content Channel View), instead of always showing the bar if user has edit rights to any block on page.
+ Added new workflow action to allow setting the initiator from an attribute value
+ Updated the communication recipient list on person history to allow senders to view any communications they created.
+ Added account summary to the transaction list block.
+ Added new "Format as Link" option to Image field types which will format the image to navigate to a full size image when the image is clicked.
+ Fixed issue with not being able to move transactions from one batch to another on Firefox.
+ Added option to Statement Generator to limit statements to a Dataview.
+ Added new badge that will display if a note exists on a person's record that is marked 'Is Alert'.
+ Updated content channel view block to refresh the view after cancelling out of the edit dialog (Fixes #1245).
+ Fixed issue with payment downloads updating the scheduled transaction detail amounts instead of the transaction detail amounts (Fixes #1250)
+ Fixed updating of scheduled transaction to not create account detail records with $0.0 amounts.
+ Removed the Output Cache Duration option from block properties as this functionality is not fully functional (Fixes #1096).
+ Added the ability for communication approvers to edit a communication before approving it.
+ Added new PrayerRequestListLava block type.
+ Updated the Send Email and Send System Email workflow actions to support multiple "to" addresses.
+ Added account list to the scheduled transaction list block.
+ Added PageRedirect lava filter.
+ Fixed database locking contention that would occur when saving a workflow with a new attribute and a user entry form (and database was configured with snapshot isolation turned on).
+ Fixed the Transaction History block to show transactions correctly for the correct person (not everyone's) when the current person does not give with their family (Fixes #1243).
+ Updated grid export to excel to export numeric values correctly (not as text values).
+ Fixed Dynamic Data Report issue when Selection URL contained more than one column name (Fixes #1238).
+ Renamed the ForeignId field to ForeignKey (string field) and added new ForeignId (int field) and ForeignGuid (Guid field) fields to every table/model.
+ Added column (and filter) to Attendance History block to indicate whether a person attended the group or not (fixes #1184).
+ Updated SMS communications to require From and Message values (Fixes #1171).
+ Updated Bulk Update to allow selecting the note type when adding a new note for people (Fixes #1165).
+ Updated the processing of scheduled payments to also update the status (Fixes #1145).
+ Added a link to Report Detail to navigate to the associated data view (if user has edit rights to report).
+ Updated the Security dialog to read values from DB instead of cache (Fixes #903).
+ Updated the Transaction Entry block to use public account names on confirmation and success views (Fixes #1220).
+ Updated the family edit to allow adding new people with unknown gender.
+ Added new Person Merge Requests page under Data Integrity.
+ PrayerRequestEntry block can now accept the text (if URL encoded) of a request via the Request parameter.
+ Fixed BirthdayPicker to display controls in correct order for ShortDateFormats that include day/month format specifiers other than "dd" or "MM" (Fixes #942).
+ Updated Person Merge so that it shows a "Merge Request Submitted" if the person is only authorized to View.
+ Added Saved Account List block that can be used by users to delete their saved accounts.
+ Fixed issue with ordering of accounts (and their amounts) on transaction entry block when accounts are added by user in different order than the default account order (Fixes #1219).
+ Modified date picker to work more like the time picker. You can now click on the calendar icon and it will open. Also added logic that if you click the calendar icon it will select the text of the date which allows you to start typing right away to change the date. This should now be consistent with the time picker. (Fixes #1189).
+ Added toggle button to Show/Hide Data View and Report Results.
+ Updated the merge fields available to exception notification system email to include a 'Person' field for the current person.
+ Added a feature to the data view detail block to show what reports and other data views are in use.
+ Updated the testing of workflow action filters to correctly compare numerical values (Fixes #1205).
+ Added setting to the transaction list block to limit transactions to one or more transaction types (i.e. only contributions on person contribution tab ).
+ Added additional filters and ability to sort to the scheduled transaction list.
+ Updated Check Scanner so that the user chooses the Transaction Source on the scanning prompt page instead of the options page. This list is limited to the Transaction Source defined values that have 'Show In Check Scanner' set to true.
+ Added setting to transaction list that allows limiting accounts in filter to be only active accounts.
+ Added column to batch list that summarizes account totals, and fixed batch notes to display on batch list.
+ Fixed the Delete Workflow action.
+ Added workflow action to create a connection request and added new controls, field types, and attributes for selecting connection opportunity, request, and status.
+ Added new Floor and Ceil Lava filters for rounding (Fixes #1193).
+ Updated email communication medium to only allow valid email address in From Address field, and updated communication send job to continue processing communications when one communication has an issue.
+ Added new Children transform for dataviews that will transform a list of parents to a list of the children of those parents
+ Updated the person save so that when a person is inactivated, their group members status will be updated to inactive also for all the groups they are members of unless the group's group type has the new 'Don't Inactivate Members' option set.
+ Added framework for adding following events and suggestions.
+ Added a Copy command for Reports.
+ Added a Copy command for Data Views.
+ Added new Lava filters for getting a person object from a PersonId and PersonAliasId
+ Added additional Payment details model/table to store the details of a payment such as currency type, credit card type, and name on card, expiration date, and billing address so that in the future functionality can be added to allow users to update their information without having to recreate scheduled transactions.
+ Added mobile redirect options to site configuration to allow automatically redirecting mobile clients to a different page or external url.
+ Updated workflow processing so that if an action fails on initial processing the workflow would still be persisted if configured to do so.
+ Added new EnableSelfSelection option to the Person Picker
+ Fixed issue with attribute value having html encoded text when they shouldn't (e.g. global attribute in email subject)
+ Updated parent check-in label to include the last name in addition to nick name
+ Modified the allergy and legal notes on check-in label to truncate instead of writing over itself.
+ Updated the grid's page setting size preference to be grid specific instead of for the entire site
+ Upgraded to FontAwesome 4.4 (66 New Fonts)
+ Updated the sending of system and workflow emails to also optionally write a communication history to the recipients profile record.
+ Fixed Exception List showing incorrect "Last 7 days" count for exceptions with empty Site Name. (Fixes #1174).
+ Fixed issue with Twilio transport sending to the same recipient more than once (Fixes #1077).
+ Rock Duplicate Finder no longer displays duplicates following merge (Fixes #985).
+ Known Relationships with deceased people now show in the Known Relationships block (Fixes #1164).
+ Added Graph By Location option to Attendance Analytics
+ Added new Communication History block to the person profile page
+ Changes to the order of person profile badges now update the badge bar without having to clear the cache (Fixes #933).
+ Google Maps link now searches for a named location's address rather than its name (Fixes #1093).
+ Added setting to the Category Tree View block to allow setting the icon to display for items that do not have an IconCssClass property.
+ Fixed issue with recurring specific dates not saving start times (Fixes #1147).
+ SendEmail workflow action now works with attributes of type Group (Fixes #1167).
+ Merging a person with a security role now refreshes that role (Fixes #1137).
+ Added ability to delete a batch and all its transactions from the Check Scanner utility.
+ Fixed issue with pre/post delete triggers not firing when a column qualifier was specified.
+ Added new Lava filter to return the possessive form of the string (Ted -&gt; Ted's, Charles -&gt; Charles').
+ Changed the Group Details block to require 'Administrate' access to view/edit Group Member Attributes.
+ New Lava filter for returning a set of next date/times from an iCal string
+ Made Batch Id visible in Check Scanner Utility
+ Added ability to delete a scanned transaction in the Check Scanner Utility
+ Added new Lava filter for returning information about the current page.
+ Added navigation improvements to MyWorkflows block
+ Added new Lava filter for returning information about the current page.
+ updated Check Scanning to prompt the user when a Bad MICR read or duplicate is detected, which the option of Uploading it as-is.
+ Added public description html and is public fields to accounts.
+ Fixed the MonthYearPicker so that it maintains current selection through postbacks.
+ Fixed GroupList Active Status filter and display to consider both the Group Status and the Membership Status when determining if a Group Member is active. Also modified to show inactive Memberships in a lighter-weight font and sorted last by default. (Fixes #1140).
+ Updated Edit Family to automatically save the address if an address is in edit mode when you save the family.
+ Workflow triggers can now be fired when a qualifier changes-to or changes-from a value for pre-save trigger types.
+ Added new lava filter PluralizeForQuantity
+ The Communications Entry block will now send an email to Communication Approvers when a new communication requires approval.
+ Add 'Between' as an option for all date dataview filters that lets you choose a date range or a sliding date range
+ Changed IsDeceased on Person from a Nullable to Not Null. Any records that have a NULL value for IsDeceased will be marked as IsDeceased=False
+ Users can now search for businesses in PersonPicker (Fixes #1047).
+ Sites can now be deleted via an extra confirmation (Fixes #937).
+ Added Spatial Indexes to the Location table which should significantly speed up the DistanceFrom datafilter plus any other GeoSpatial queries
+ Added options to the "In Group" person data filters that let you specify "Include Child Groups" with options of "Including Selected Groups" and "Include All Descendants" (which default to true if "include child groups" is selected)
+ Datafilters for booleans now have an "Equal To" and "Not Equal To" comparison option. Handy for nullable booleans and boolean attributes. For example, if you want to filter on 'Has a Pet Cat', 'Not Equal to True' will include people that either have False or don't have a value for 'Has a Pet Cat'
+ Added new REST endpoints for editing Attribute Values.
+ Added new REST endpoints: api/People/AddNewPersonToFamily and api/People/AddExistingPersonToFamily
+ Added display of current room counts to the check-in manager login screen.
+ Updated attendance analytics block to include DataView filter.
+ Added optional setting to FindFamilies checkin workflow action that allows phone search to only include families with a phone number that ENDS WITH the value entered during check-in rather than all the families with a phone number that CONTAINS the value entered.
+ Updated new family control to use correct phone type labels rather than hard-coded 'home' and 'cell' phone labels (Fixes #941).
+ Updated checkin to persist the theme selection in browser's local storage so that if theme is changed, it is remembered on subsequent starts.
+ Added new Lava filter to transform a property into a key value pair.
+ Added workflow action to post attendance to a group.
+ User Logins can now be added from the Security &gt; User Accounts page (Fixes #888).
+ Refactored note types so that new (and existing) note types can be configured and secured.
+ Added new workflow action to set an attribute for a group member.
+ Added new workflow action to set a person attribute to the leader of a provided group.
+ PersonName linked field in Report now appears correctly and is an Excel hyperlink (Fixes #1063).
+ Added a calendar feature for events.
+ Added Gender, Age, and Grade to the Check-in Manager person profile display.
+ Fixed issue with Check-in Manager that would cause navigation options to disappear when switching campus context.
+ Updated checkin to prevent multiple options (family, person, group type, etc.) to be clicked on.
+ Added new workflow action to create a person note.
+ Improved check-in performance.
+ Improved performance of Check-in manager.
+ Turned off auditing by default (Can be turned on by adding a boolean 'EnableAuditing' global attribute).
+ Added 'Public' option for Groups that can be set to false to make the group a private group, which will prevent it from showing in the group finder.
+ Updated Transaction List, Batch List, DataViews and Reports so that the grid will only fetch the records it needs. This will help the grids load quite a bit faster.
+ Improve performance of attendance list/detail blocks
+ Add Location path to attendance list/detail blocks
+ Add option to add new attendees to attendance detail block
+ PersonPicker now provides some feedback if the user is unauthorized to search (Fixes #1042).
+ Added Lava filter 'DateAdd' to add a number of days/hours/minutes/seconds to a date.
+ Added new PIN Authentication that can be used as a special "PIN Only" login for check-in management.
+ Changed Group Tree View so groups will be shown regardless of the 'Show In Navigation' group type setting if specific group types are configured in the block settings.
+ Updated group editor to prevent changing parent group type to a group that does not allow the current group's type.
+ Updated check-in so that people without a birth date cannot check-in to groups that have an age range requirement (this can be reverted through a new 'Require Age' workflow action setting.)
+ Added the option to the group type check-in block to optionally automatically select all group types and then display the groups for all matching group types.
+ Added Lava filter for randomizing arrays (useful for shuffling ads).
+ Added SetPageTitle Lava filter.
+ Added the ability to change the page title from the Dynamic Data block.
+ Updated the Transaction List Liquid block to update status of a giving profile correctly when inactivating the profile.
+ Added full location path to location column in Check-in Schedule configuration.
+ Fixed communications through SMTP and Twilio to clean up any special MS Word characters that might have been pasted into the Subject line, or Body, or Message.
+ Added a special Attendance Grid to AttendanceAnalysis that will show a person's attendance per Week/Month/Year as checkboxes.
+ Added new Lava filter to get groups that a person has attended.
+ Added ReplaceLast Lava filter useful for replacing the last comma with 'and'.
+ Added HtmlDecode Lava filter.
+ Added new Lava filter to return the group(s) of a specific type that person lives within the geofenced boundary of.
+ Added new Lava filter to return the members of a particular role in the group of a specific type that person lives within the geofenced boundary of (i.e. neighborhood area leader).
+ Added new Lava filter to return the nearest group for a person.
+ Modified Group Detail block to only display attributes with "Show in Grid" flag enabled.
+ A person's previous last names can now be edited in edit person.
+ Moved Giving Group and Previous Names UI to be in an "Advanced Settings" panel in edit person.
+ Searching for a person will now look for any previous last names that the person has had.
+ Added 'Date Added' to group members.
+ Added new Send Birthday Email job.
+ Added Jobs to calculate Metrics and calculate Group Requirements
+ Added new 'Format' Lava filter to help format numbers.
+ Fixed issue where Data View comparison "Is Not Blank" incorrectly returns Null values as matches. (Fixes #948).
+ Added new Transaction Yearly Summary Lava block that can be used as standalone or as a person profile block.
+ Added the group leader as a tooltip on the Geofenced By Group person badge.
+ Fixed issue where Excel Export from Grid incorrectly shows Defined Value Fields as Guids (Fixes #935).
+ For REST, changed DateTimeZoneHandling to Unspecified instead of the default of RoundTripKind since Rock doesn't store dates in a time zone aware format. Since Rock doesn't do time zones, we don't want Transmission of DateTimes to specify TimeZone either. This fixes an issue where the batch date of scanned checks could be off by a few hours in certain edge situations, and prevents similar issues from happening.
+ Return the created entity after a REST POST to improve the efficiency of 3rd party software using the REST endpoint
+ Added an Active|All toggle filter to the Group Tree View that will either show only Active (the default) or All groups. The filter can be hidden in the Group Tree View block settings which will hide the filter and only show Active groups in the treeview.
+ Added a new person badge type that can display any groups of a particular type that have a geo-fence location that surrounds the person's family address (geopoint).
+ Added a new 'Volunteer Check-in Area' to make it easier to configure serving teams for check-in.
+ Updated the check-in configuration block's schedule and group/location editors to work more consistently with groups that are also configured and managed using the normal group viewer.
+ Added new Workflow Actions that can be used to add a Person to a Group
+ Added a new Lava filter for person objects to get all groups of a selected type that user belongs to.
+ Updated the Workflow Detail block to check both the block security settings and the workflow security to determine if user is authorized to edit a workflow (If user has edit rights on either, they will be allowed to edit the workflow)
+ Fixed Entity Type filtering on attribute category block and updated attribute list block to correctly display categories when adding/editing an attribute.
+ Updated workflow form processing so that if user has edit rights to workflow type, they will be able to process workflow forms even if they are not the one that form is assigned to.
+ Updated group attendance list/detail blocks to support groups with multiple locations and schedules (i.e. groups used for check-in)
+ Added a binary file picker attribute
+ Updated notes block to allow backdating notes
+ Updated Group Type Detail block to display child group type list and picker in correct group type order.
+ Updated prayer request comment list default sort order to be descending date order instead of ascending (show most recent first).
+ Updated Communication List block to use Create Date for date range filtering instead of Reviewed Date.
+ added a PATCH method to the Rock REST API. Patch allows any entity's properties or a subset of those properties to be updated.
+ Improved the performance of the Communication History block
+ Timepicker now figures out the time if you type in the time such as 2p, 11a, 345p, 1155a
+ Added Page Parameters as merge fields to the Dynamic Data block. For example {{ PageParameter.GroupId }} would give you the GroupId from the query string.
+ Added a javascript helper method for setting Rock Context and added support for setting Rock Context through a query string parameter (e.g. Campus Context).
+ Fixed Transaction Matching not showing the Person Details if the person was selected using the Person Picker. Also made it more obvious which person the transaction is going to be assigned to if a new person is selected instead of the automatically assigned person and vice-versa.
+ Updated transaction matching so that pressing Enter will navigate to the next record.

Rock McKinley 3.5

+ Fixed issue with occasional website hang
+ Fixed issue with ordering of accounts (and their amounts) on transaction entry block when accounts are added by user in different order than the default account order (Fixes #1219)
+ SendEmail workflow action now works with attributes of type Group (Fixes #1167).
+ Merging a person with a security role now refreshes that role (Fixes #1137).

Rock McKinley 3.4

+ Improved performance of caching.
+ Improved performance of Check-in manager blocks.
+ Improved performance of attendance list and detail blocks.
+ Turned off auditing by default (can be turned back on by changing the value of 'Enable Auditing' global attribute).
+ Updated PersonPicker control to provides some feedback if the user is unauthorized to search (Fixes #1042).
+ Updated performance of First Contribution Date report filter (Fixes #972).
+ Updated Group Finder block to not display inactive groups (Fixes #1019).
+ Fixed Pledge List block so that it correctly applies the Account filter when no accounts are selected (Fixes #1032).
+ Added support for financial gateway providers to specify whether they support Saved account functionality for a given currencty type ( credit card, ACH ) and updated UI to render options based on setting (Fixes #1031).
+ Fixed Family group members getting added with inactive status (Fixes #1013).
+ Improved performance of Rock Cleanup job (Fixes #1060).

Rock McKinley 3.3

+ Fixed exception in Batch Detail when batch contained transactions without a currency value (Fixes #998)
+ Fixed issue with person history causing an exception after manual attendance was entered (Fixes #999).
+ Fixed exception on Page List when attempting to sort by Layout column (Fixes #1004)
+ Fixed issue with address auto-compute not working correctly on the person search (Fixes #994)
+ Fixed issue with 'First Contribution Date' report filter taking a very long time to load (Fixes #972).
+ Updated the link in group attendance email to include the impersonated person link so that group leader does not need to login when entering attendance (Fixes #1000).
+ Updated Template Detail block to use 'SMS' instead of 'Sms' (Fixes #891).
+ Updated Person Following List to show proper home/mobile phone in heading based on value of the home/mobile defined type (Fixes #1003).
+ Updated Person Following list so that phone and spouse columns are not sortable (Fixes #1001).
+ Updated check-in to print labels in configured order (Fixes #1007).
+ Updated the person merge so that it does not set external logins to be unconfirmed (Fixes #1005).

Rock McKinley 3.2

+ Fixed Facebook login to match with existing person correctly (Fixes #983).
+ Fixed exception that would occur when trying to activate a new activity on an existing workflow (Fixes #978).
+ Updated the Checkin Schedule to have it's 'StartTime' property available to Lava (Fixes #981).
+ Updated the default ordering of transactions in Contributions tab of person profile be datetime of the transaction (Fixes #980).
+ Fixed issue with My Workflows block not showing the workflow type.
+ Fixed "Group Type" Report Field not showing any results (Fixes #977).
+ Added validation to prevent duplicate routes being created for different pages.
+ Updated person object so that PrimaryAlias property is avaialbe to be used by Lava (Fixes #973).
+ Fixed issue with Add Family block adding children as adults (Fixes #964).
+ Fixed issue with prayer approval not updating request correctly, resulting in prayer not being visible to prayer session (Fixes #969).
+ Updated communicaiton processing so that communication job will not send duplicate emails to same recipients.
+ Fixed issue with occasional deadlocks.
+ Added job for processing scheduled metrics.
+ Fixed attribute block to escape HTML in the values column (fixes #965).
+ Added access keys to edit the individual (Alt+I) and family (Alt+O) on the person details page.
+ Changed the access key for edit from Alt+E to Alt+M (modify) since Chrome already used Alt+E. (Fixes #946).

Rock McKinley 3.1

+ Improved performance of check-in's Find Families action.
+ Added a setting to the check-in Find Families action to limit the number of families returned by search (default is 100).
+ Added a phone number index to improve check-in performance.
+ Fixed filtering issue when filtering on defined value attributes that allow multiple values (Fixes #910).
+ Updated the Transaction Entry block so that the 'Next' button does not remain disabled after a validation error (Fixes #899).
+ Fixed issue with not being able to send communications that included an attachment.
+ Fixed issue with displaying known and implied relationships on the person detail page (Fixes #930).
+ Fixed spacing of the 'In Group of Type' badge (Fixes #908).
+ Fixed issue with existing people creating a new login and not being able to login (Fixes #928).
+ Improved how Rock manages shared connections to database.
+ Fixed issue with tag report.
+ Added additional indexes to improve performance of person merge.
+ Fixed issue with not being able to update a binary file person attribute (Fixes #874).
+ Fixed issue with SMS replies being sent to the wrong person.

Rock McKinley 3.0

+ Updated the PayFlowPro transaction download to exclude 'declined' transactions
+ Fixed inaccurate check-in countdown due to time not using ISO 8601 format (Fixes #878).
+ Made some changes to improve performance and accuracy of check-in random code generation.
+ Fixed BusinessDetail block so that an exception does not occur when trying to delete contacts from a business (Fixes #867).
+ Added a job and system email for sending reminders to group leaders about entering attendance for their group meetings.
+ Updated SampleData block and xml data to handle adding small group topics as  defined values, and to handle setting up a small group's schedule.
+ Updated the Add Family, Edit Person, and Edit Family blocks to have a more consistent UI for adding new people (Fixes #849, Fixes #850, Fixes #855)
+ Fixed issue with effective end date not being updated when saving a recurring schedule.
+ Updated the zone block list so that it only copies authorization from page when a new block is added and not when an existing block is edited.
+ Updated LoginStatus block to only show "Login" when user not logged in, and not an empty dropdown
+ Updated person following and person attribute filtering to use person alias id instead of person id.
+ Updated the Transaction Detail block to facilitate easier keyboard adding of transactions.
+ Changed PrayerRequestDetail block to not encode HTML prior to storage as per https://github.com/SparkDevNetwork/Rock/wiki/Security-Considerations (Fixes #846).
+ Updated the communication send process to use the the person who created the communication when resolving merge fields so that secured attribute values can actually be included in a communication (Fixes #802).
+ Fixed display of transaction list/detail when user is authorized to view, but not to edit (Fixes #805)
+ Updated Bing address verification to handle a null postalcode value returned for valid international addresses (Fixes #845).
+ Updated the boolean field type to render a drop-down list with a blank option instead of a radio-button. This allows specifying a none/true/false value. If attribute is configured to be required, user would still need to select true or false (Fixes #776, Fixes #784)
+ Updated Family Edit to set email active when adding new family members (Fixes #820).
+ Updated the HTML Editor's merge field picker to use current person's security when determining the attributes to display and include in list ( vs. showing only the attributes available to everyone) (Fixes #802).
+ Fixed issue with note container only showing first 10 comments and not displaying the 'Show More' option (Fixes #808).
+ Added a new api/Groups/SaveAddress REST endpoint for updating the address associated with a group/family. This method takes care of checking for existing addresses, updating address pointers correctly, and writing change to family history.
+ Updated the api/Peoples REST POST endpoint to create all the needed relationships for a new person (i.e. Family Member, Person Alias, Known Relationships, Implied Relationships)
+ Fixed the TransactionEntry block to update the phone numbers and/or email address for an existing person.
+ Updated the Communication Entry block to support a 'Simple' mode that can be used on an external site for members to initiate a communication.  Also updated Site to include a Communication Page setting.
+ Changed directory names in the content folder to not have spaces.
+ Added a 'Schedule' field to the group detail that allows a group to have either a simple weekly schedule, a custom schedule, or use any one of the named schedules
+ Updated the 'First-time Icon' merge field for check-in labels to use a new Person.FirstTime field that is true if person has ever attended anything. This replaces previous merge field that only checked if person had previously checked into a group with same group type.
+ Added regex option to the search screen of check-in to allow applying a regex pattern to the user's input. For example the expression '[0](\d*)' would strip off a leading 0 from the input. This example would be needed in the UK where Rock does not store the leading 0 in the database but many individuals would be used to searching using it. #MakingUpForTheSinsOfTheNSA
+ Added option to include people without a mailing address in the Statement Generator. Also, if an individual person is selected, it will include them regardless of mailing address.
+ Updated Statement Generator to recommend a 600x200px logo, and to ship with a standard logo at 600x200px.
+ Add new 'My Settings' page which lives under the 'Login Status' drop-down in the upper right of the internal pages. This page will be a user's one stop shop for all of their personal settings and configurations. For now it contains links to change their password and manage their communication templates.
+ Attribute and AttributeValues can now be included in a REST response by specifying ?LoadAttributes='simple' or ?LoadAttributes='expanded' in the request parameters. In 'simple' mode, only the most commonly needed properties of Attributes and AttributeValues are serialized in a REST response.
+ Updated the 'Set Attribute From Entity' workflow action to support setting a value based on a Lava template that can parse the entity passed in for processing.
+ Moved the Content Channel Type and Content Channel admin screens from under the 'Communications' page to the 'CMS Configuration Page'. This will simplify the security access since these pages will be used more by Web Administrators.
+ Moved the 'Photo Request' page under 'Admin &gt; Communications'. This helps limit who can send out bulk photo requests and helps to clean up the 'People' menu item.
+ Adjusted the security under 'Admin Tools' to allow the 'WEB - Administration' security role to see the 'CMS Configuration' pages and the 'RSG - Communications Administration' role to see the 'Communications' pages.
+ Added edit and administrate access for the 'WEB - Administration' role to the External Website Ads content channel.
+ In an effort to help organizations create an organized system for managing their security roles we've implemented a sample naming system. You can read more about our recommendations in the updated Admin Hero Guide.
+ Added new DateTime field type for attributes.
+ Fixes DISC Last Save Date not storing in international date format (Fixes #828).
+ Upgraded to FontAwesome 4.3.0 (Fixes #814)
+ Added the ability to filter group members by specific attribute values
+ Added additional checking in the PhotoSendRequest block to verify the communication template and certain fields in the template (Fixes #798)
+ Fix new REST key not confirmed on create and UserDetail block showing incorrectly that it was. Fixes https://github.com/SparkDevNetwork/Rock/issues/825.
+ Added Block Property to GroupList to filter by Active Status. If this is set, it overrides the user filter for Active Status
+ Fixed check scanner allowing checks without a check number through when using a Ranger scanner
+ Added option in Address data select to specify what portion of the Address to show in report.
+ Added an option to the 'Memo' field type to optionally allow html to be entered in field without causing a Validation Request error.
+ Added an option in Service Job list to "Run Now".
+ Updated the grid export to resolve defined value ids with their values when exporting to Excel.
+ Changed Statement Generator to display a breakdown of the Accounts for each Transaction
+ Changed the DISC service to work even if someone changes the DISC category name (Fixes #792).
+ Changed reverse order and format of the graduation and grade labels on the person profile (Bio block)
+ Added "Combine Giving" option to Total Giving Amount data select in reporting. When enabled, the total giving for the person will include amounts given by members of their giving group (or just the total for the person if they are not in a giving group)
+ Rearranged the Content Channel Item View block to make it more intuitive. (Fixes #787)
+ Fixed issue with Location Picker control not maintaining state when toggling between different location types on the group details location editor.
+ Added keyboard shortcuts to speed up several operations in Rock. These include:
   - Alt + Q = Quick Search (sets focus to the search box at the top of the page)
   - Alt + S = Save (presses the save button on the given page)
   - Alt + E = Edit (presses the edit button on the given page)
   - Alt + N = New ( presses the add button on any grid on the page)
   - Alt + C = Cancel (presses the cancel button on the given page)
+ Added new Lava filter to create postback links in Lava.
+ Fixed the Delete workflow action so that it does not cause an exception when processing.
+ Updated the workflow detail block so that only users with edit access to the block see the advanced options for editing a workflow.
+ Removed the double borders around the grid when used inside of a grid-panel.
+ Fixed an issue with Categories block that prevented categories from being able to be ordered.
+ Added new block attribute types for using the KeyValueList and ValueList field types
+ Added a new rating control and field type. An attribute can use this field type to display a group of stars that can be selected to rate an item.
+ Add new note types for use in workflows.
+ Updated the 'Set Attribute From Entity' workflow action to include an option for determining if workflow should continue processing or not when the entity is missing or invalid.
+ Fixed bug in transaction detail block where the cancel button would not take you back to the parent batch when clicked.
+ Made changes to the transaction details block to make entering transactions faster. There is now a new 'Add New Transaction' button to enter an additional sibling transaction and the person picker now expands when entering a new transaction.
+ Changed the default sort order of financial transactions to be in the order they where entered.
+ Changed person picker to set the focus to the search box when opened.
+ Added an option to reuse the code per family (disabled by default).
+ Updated the Binary File URL property to return a full path instead of a relative path when file type is using Database or File System storage provider.
+ Updated check-in labels so that attribute values are not HTML encoded.
+ Added helper method to SavedAccount model to create a new reference transaction
+ Added an "Authorize" method to financial gateway components.
+ Fixed issue that enabled the keyboard to display on touch devices (Fixes #777).
+ Updated content feed to sort content items in descending order. Also added filters to respect the content item's date ranges.
+ Updated workflow administration so that users with edit security on a workflow type can manage workflows of that type (Fixes #771).
+ Updated workflow list to allow filtering by workflow attribute values
+ Added feature to the GetChannelFeed handler to resolve any relative links to absolute links.
+ Added block setting to the Bio block used on the Person Profile page to enable prepending the country code to phone numbers. Helpful for internationalization.
+ Updated block rendering so that the Pre/Post text is rendered outside of all the block wrapping divs
+ Updated the Lava "Attribute" filter to use the current person when checking security rather than defaulting to anonymous user (Fixes #760).
+ Fixed the 'Forgot User Name' block to include Active Directory user names (Fixes #765)
+ Updated the FinancialPersonSavedAccount to also allow being associated to a group in addition to a person
+ Fixed issue with static text being replaced with security code on check-in labels (Fixes #766).
+ Updated the address verification services so that a second active service will attempt to standardize/geocode a given address even when previous active service attempted to standardize or geocode the address but failed.
+ Updated the Defined Value field type to optionally display defined value descriptions instead of the values.
+ Added ability to add notes to a workflow
+ Fixed exception that would occur when manually activating a new activity on an existing workflow.
+ Fixed the workflow processing job so that it will still process workflows when the workflow type has a blank processing interval.
+ Updated the logging of exceptions so that the source and stack trace information get logged correctly for inner exceptions.
+ Added 'Exclude Group Type' block setting to Group Tree View and Group Detail
+ Added new group type for tracking internal staff organizational structures with a new page for viewing them.
+ Added the ability to specify root document/image folders and the option to optionally force user-specific root folders for attributes that use the HTML field type
+ Fixed the 'Delay' workflow action to correctly calculate the time that delay was initiated.
+ Changed Google Map API so it only loads when it is needed vs loading on every page. If you have any custom blocks that need the Google Maps API, call this.LoadGoogleMapsApi() in your OnInit.
+ Added option to CategoryTreeView to set the Root Category
+ Updated the Rock Grid control to handle separate DataKey and PersonId columns correctly.
+ Added option to use CurrentDate plus/minus a number of days in DataView date filters
+ Updated the Workflow Entry block so that it will interrogate the query string parameters and set any workflow attributes with same key to the value passed in query string. This results in the Activate Workflow block no longer being needed, so it has been deprecated and will be removed in a future update.
+ Updated workflow type configuration block to include the attribute field types in grid of workflow and activity attributes.
+ Add support for the ExcludedByFilter flag to RemoveEmptyPeople workflow action
+ Updated the Facebook authentication provider to update the person's Facebook person attribute value whenever they login.
+ Added the ability to specify the kiosk id and group type ids as a query string (route) parameter so that the check-in admin screen can by bypassed.
+ Added Lava Address filter option to provide a template for the address return.
+ Add Scan Settings to BinaryFileType
+ Updated the 'Attribute' lava merge field to return an object (instead of string) and added an additional 'Property' filter so that these can be used to navigate complex object/attribute structures when outputing lava content.
+ Updated blocks that provide 'Enable Debug' setting for Lava to only show the debug information if the logged in user also has Edit access to the block. This allows an Admin to enable debug on a production page without everyone seeing the debug info.
+ Fixed the DISC assessment block which was not working in IE 10 (Fixes #732).
+ Added responsive table support to the Rock Grid
+ Added column to GroupMember list for linking to profile detail (and removed 'View Profile' button from group member detail)
+ Added new Lava filter 'Default' that returns the passed default value if the value is undefined or empty, otherwise the value of the variable.
+ Updated several of the Lava string filters (Replace, ReplaceFirst, Remove, RemoveFirst, Append, Prepend to accept objects instead of strings. This allows these filters to be used on numbers also.
+ Updated the Facebook authentication provider so that it works with the new Facebook API.  If person does not have existing photo in Rock it will now also add a photo to Rock from Facebook, and will add a new 'Facebook Friend' known relationships for any of the user's Facebook friends that have also previously logged into Rock using Facebook (Fixes #675).
+ Updated the Memo field type so that attributes of this type retain their line breaks when displayed on page (i.e. workflow entry screen, workflow detail, etc.)

Rock McKinley 2.0

+ Fixed the EmailHeader and EmailFooter global attributes to have correctly formatted HTML content (Fixes #740).
+ Fixed the Check-in countdown until active display when there is an earlier schedule for the same day that is no longer active (Fixes #739).
+ Fixed an issue with not being able to update a Multi-Select attribute value in the Bulk Update block (Fixes #734).
+ Fixed the Attended Duration badge to correctly include Sunday attendances.
+ Fixed the weekly attendance grouping in Attendance Analysis so that Sunday attendances are grouped to the correct weekend.
+ Fixed attendance and DISC badges in IE.
+ Fixed a bug in the lava debug display that kept some attribute help from showing the parent level object.
+ Fixed the DISC assessment block which was not working in IE 10 (Fixes #732).
+ Added CreatedByPersonName, ModifiedByPersonName, CreatedByPersonId, and ModifiedByPersonId as available lava merge fields for all models.
+ Updated security on groups so that a group can use both its parent group and its group type as a parent authority. Also improved security handling in the Group Viewer block (Fixes #718).
+ Fixed security so that if a user is not allowed to view a Group type, they cannot view/edit groups of that type in the Group Viewer block (Fixes #719).
+ Updated the Day of Week attribute field type to allow setting a default of none (Fixes #726).
+ Updated Person Merge and Bulk Update to include deceased people when navigating from grid of people (Fixes #727).
+ Updated the Social Media person attributes to use a URL field type instead of just text (Fixes #728).
+ Updated all emails ( system, workflow, etc) to check that sender is from a "Safe Sender" domain.
+ Updated all the places that use "Person" as an available lava merge field for the current person to use "CurrentPerson" instead.
+ Updated the lava fields used by the PageMenu block to have the same format as all other lava merge fields in Rock. They are now mixed case fields. This required all Page Menu templates and template include files to be updated (update should automatically fix all of the templates).
+ Fixed issue with row selection not working on Dynamic Data Block.
+ Added ability to delete channel items from the Tools &gt; Content page.
+ Updated Twilio SMS transport to report back messages that are undeliverable (Fixes #715).
+ Fixed HTMLEditor issue in IE when toggling between source and WYSIWYG modes not saving content (Fixes #588).
+ Added "Include Child Groups" option to the "In Group" and "Not in Group" Person dataview filters.
+ Updated several workflow action fields used to select attributes to only display attributes that have the specific field types allowed by the action.
+ Updated the location and naming convention of the Lava include files to be RockWeb/Themes/[Theme]/Assets/Lava/[Template].lava instead of RockWeb/Themes/[Theme]/Assets/Liquid/_[Template].liquid.
+ Updated prayer requests so that new prayer requests that were not auto-approved will now show up in the Prayer Requests list without having to check 'Show Expired Requests' (Fixes #685).
+ Added DISC Result block for viewing results after clicking the DISC badge, and added the DISC Request person profile action with corresponding Workflow.
+ Added additional 'Text Option' settings to the financial Transaction Entry and Scheduled Transaction Edit blocks to make more of the text captions configurable.
+ Updated the Send Email, Send SMS, and Send System Email workflow action types so that the recipient field supports Lava.
+ Fixed attribute category ordering on person profile page (Fixes #716).
+ Updated the System Info block's 'Clear Cache' option to also delete all the files in the App_Data/Cache folder.
+ Renamed the 'Send Email Template' workflow action to 'Send System Email' since it is used to send a system email, and not a communication template.
+ Fixed issue with workflow entry form notifications including fields that are not marked visible.
+ Updated the Rock REST API so that exceptions that occur when using the API will now return an error and log the exception to the Rock Exception Log.
+ Updated communication templates to correctly use 'Communication.MediumData.*' instead of 'Communication.ChannelData.*' lava merge fields.
+ Fixed the group member status field label on bulk update block to have the correct label (Fixes #708).
+ Added block properties to the Register block (AccountEntry.ascx) to set the Connection Status and Record Status used when creating new individuals. Default values are 'Web Prospect' and 'Pending' (Fixes #699).
+ Fixed the 'Copy Communication' option in Communication History so that it correctly copies all the channel data (e.g. email body text) to the new communication.
+ Subject value in email templates can now be removed (Fixes #692).
+ Updated workflow entry screen so that inactive workflow types cannot be used to start a new workflow. They will still appear, so that they can be managed, but will not link to the entry form (Fixes #695).
+ Fixed issue where public account names were not being used on the transaction entry page (Fixes #696).
+ Updated Lava templating engine to support enumerations correctly (i.e. Gender) (Fixes #689).
+ Fixed several problems associated with Global Attributes not being properly merged in communication templates for email exceptions, account confirmation, and account creation during new giving transaction (Fixes #684).
+ Updated the Group Tree View block so that it only shows groups of the selected type when specific types are selected in the settings (Fixes #671).
+ Dataview filter selector now shows tooltip with the description of the filter.
+ Report field selector now shows tooltip with the description of what the field is (if it has a description).
+ Changed "Person Link" report field to "Person Name" and moved it to show in the Common fields. It also now has the option to show as a link, and whether to show 'FirstName Lastname', or 'LastName, Firstname'
+ Fixed issue with adding multiple existing people to a family not displaying the correct tab in add person dialog window (Fixes #666).
+ Updated the check-in application so that when the search page is displayed, the number field already has focus (Fixes #623).
+ Fixed issue with check-in codes not printing on labels.
+ Added a way to set the requester of the prayer request on the Prayer Request detail block. Also now shows the requester if one was set.
+ Fixed communication copy so that it uses the current datetime instead of retaining the original create date (Fixes #651).
+ Fixed bug that was preventing GlobalAttributes from being included in new account creation block emails (Fixes #649).
+ Updated Rock REST API to use latest version OData V3. Now there is support for most of the standard OData V3 Query Types and also an endpoint at api/$metadata to see the schema
+ Fixed issue that was preventing prayer requests with comments from being deleted until each comment was removed first (Fixes #644).
+ Fixed a few REST endpoints that were not configured to check authentication correctly (Fixes #647).
+ Added workflow actions to add and remove a person from an organization tag.
+ Fixed exception that would occur on person bio block if person had a phone number with an invalid phone number type
+ Added new Video and Audio field types that can be used to display Video and Audio content.
+ Updated the 'Set Attribute From Entity' workflow action to support any entity type instead of just person or group.
+ Fixed issue with attribute values becoming unavailable when a cached item is reloaded from a model
+ Updated the Memo field type to have a configurable number of rows (instead of always being 3).
+ Added a new DISC Person Profile Badge.
+ Removed the 'Current Date' check box option from the Date field type and added it to a new FilterDate field type. This field type is intended to only be used for report filters when specifying date comparisons (Fixes #627).
+ Added "Not in Group" and "Not in Group of Group Type" dataview filters.
+ Added new "Media File" BinaryFileType as the File Type for Video and Audio field types.
+ Added new AudioFieldType that can be used to add Audio content.
+ Added New VideoFieldType that can be used to upload and include video in content pages.
+ Updated the MyWorkflows block so that when the 'All Types' option is used, only the workflow types that user is authorized to edit are displayed (in addition to any types that have active activities assigned to current user).
+ Updated the 'Person/{PersonId}/StaffDetails' route for the person profile security page to be 'Person/{PersonId}/Security' instead.
+ Removed the 'Blocs' page route as it is no longer needed.
+ Updated how lava include files are referenced. Previously they always needed to be in the current theme's Assets/Liquid folder, and required an underscore prefix and suffix of 'liquid'. Now they are referenced using the actual path and file name relative to the website's root folder.  For example a previous include of {% include 'PageNav' %} would now be {% include '~~/Assets/Lava/PageNav.lava' %}. All of Rock's Liquid include statements have been updated to reflect this change.
+ Updated Content Channel functionality to allow channel-specific item attributes.
+ Updated the item list on channel view block to include columns for the attributes that have been configured with the 'Display in Grid' option enabled
+ Added a new workflow action and webhook for processing background checks.
+ Updated the System Info block's clear cache option to also refresh the list of EntityTypes, FieldTypes, and BlockTypes.
+ Added new Encrypted Field Type and Attribute that can be used to store sensitive attribute values as an encrypted value in database (e.g. Passwords, SSN, etc ).
+ Added option to RockTextBox field type for using password mode on rendered textboxes and update SMTP settings and Payment Gateway settings to use password mode (Fixes #426).
+ Added a 'Delay' workflow action that will delay successful execution of action until a specified number of minutes have passed.
+ Updated the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Added optional setting to Group Member list to allow the Member Count column to be hidden

Rock McKinley 1.2

+ Fix report data view filters that use an attribute date filter (Fixes #629)
+ Include last name of family members if the last name is different than the current person being viewed (Fixes #618).
+ Fixed issue with StatementGenerator that was causing it to only work on x64 machines (Fixes #613).
+ Fix person age calculation to not be dependent on current culture being in mm/dd/yy format (Fixes #607).        
+ Fix the ordering of Content Channel Dynamic items when retrieving items from cache.
+ Update the Content Channel Dynamic block so that it does not default to a specific channel when first added to a page.
+ Update ordering of defined type attributes to clear the attribute cache so that editing values immediately reflects the updated order.
+ Turn off form validation for the field that is returned by Mandrill to Rock's Mandrill webhook so that exceptions do not occur when Mandrill posts this event data.
+ Fixed issue with the date picker when used at the bottom of screens (Fixes #592).
+ Update the Content Channel Dynamic block to set the correct liquid template folder prior to looking for liquid template files.
+ Update Check-in Configuration block to save new group types (Areas) with correct default options for being used by check-in application (Fixes #610).
+ Made the Dynamic Content Channel feed feature work off of liquid templates. NOTE: The address of the handler changed from GetChannelRss.ashx to GetChannelFeed.ashx to make it more reusable. There is also a new defined type for storing liquid templates in. See documentation for more information.
+ Update the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Update the Zone editor so that it refreshes the page after adding, removing, or changing the order of blocks on a page.
+ Fix the age and anniversary on person profile page, and the birthday picker on person edit page to be culture aware when formatting date display (Fixes #607).
+ Correct bug that was not saving the first attribute value on new group members (Fixes #606).
+ Family can now be deleted if there is only one member and that member is in at least one other family (Fixes #581).
+ Update the System Information's Clear Cache option to also clear all the cached security authorization rules.
+ Fix issue with attribute images being saved as temporary images (and therefore subsequently being deleted by clean-up job).
+ Update how cache items are cleared in the System Information's Clear Cache option. It now deletes the entire caching object instead of attempting to remove individual items.
+ Fix the Assign Activity to Group workflow action to correctly assign the activity to the selected group.
+ Fix exception caused when viewing a single select field with no value (Fixes #602).
+ Update the Person Profile page to check for 'Edit' security before allowing edits to the person, family, and relationships.  Also add Staff and Staff-Like roles to have the ability to edit by default (Fixes #597).
+ Update Google maps to load over both http and https (Fixes #593).
+ Update check-in location active time, and mandrill notices to handle time zone time comparisons correctly (Fixes #594).
+ Fix the workflow text/attribute drop down control to allow html in the text portion of the control (Fixes #591).
+ Fix issue that would cause 404 page to fail XSS validation test (Fixes #563).
+ Fix GivingGroup not getting set in EditPerson (Fixes #599).
+ fix address control getting an error if the Country doesn't have any States defined (Fixes #587).
+ Updated the Content Channel Dynamic blocks on the external site to use the themes liquid files (as an include file).
+ Updated the Group Detail block so that GroupType can only be edited when adding a new group (Fixes #605).
+ Fixed auto discovery link for RSS channels

Rock McKinley 1.1

+ Added the option for specifying 'Current Date' when adding data view or content channel item filters on date fields or attributes.
+ Updated default format on content channel item detail block to check for missing image (Fixes #580)
+ Added the option to support additional filtering based on query string parameter values to the Content Channel Dynamic block
+ Fixed the Social Media person attribute category to have the correct qualifier (Fixes #579)
+ Added communication template content that was deleted in the v1.0 update
+ Fixed issue with content channel items getting saved with incorrect status when user is authorized to approve channel items. (Fixes #576)
+ Updated check-in administration screen to exclude any group types that do not have the 'Takes Attendance' option enabled when selecting the 'Check-in Areas' to use for check-in. (Fixes #559)
+ Added preview dialog to Html Content Approval
+ Added support for merging liquid fields in the RSS channel HTTP hander.
+ Added new content channel type of 'Blogs' along with a sample blog on the external website. Also added the 'Bulletins' channel type.
+ Added new Lava filter 'ToString' to convert integers to strings.
+ Removed the homepage link from the stark theme as the logo already acts as a home button.
+ Added security to the 'Web Ads' content channel to allow 'Communication Administrators' to be able to Edit/Administrate/Approve. (Fixes #568)
+ Fixed issue that was keeping content from showing in the ContentChannelDynamic block if you were not logged in. (Fixes #567)
+ Fixed typo changing 'Nick Name' to 'Nickname'. (Fixes #570)
+ Fixed issue that limited the editing of attributes on the Person Profile screen in Firefox. . (Fixes #573)
+ Added the ability to change the page title, override the meta description, add an RSS autodiscover tag and add meta images on the ContentChannelDynamic block.
+ Added ability to sort reports on any column (except for Liquid).
+ Fixed DataView filters to work on MultiSelect Attributes.  For example, if the Attribute is "Favorite Colors" with options of "Red,Blue,Green", a data filter of "Favorite Colors is Blue or Red" will work.
+ Update HTML Content Detail block to include merge fields even when there is not a logged in person.
+ Update Dynamic Data Block to support column sorting, and multiple result sets.