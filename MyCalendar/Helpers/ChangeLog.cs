using System.Collections.Generic;

namespace MyCalendar.Helpers
{
    public static class ChangeLog
    {
        public static Dictionary<string, IList<string>> GetChangeList()
        {
            var changes = new Dictionary<string, IList<string>>
            {
                {
                    "v1.2.2", new List<string> {
                        "Add website background and other style changes",
                        "Only use iCalendar authentication cooking removing Cronofy cookie to authenticate",
                        "Move cronofy authentication link in Settings and show if account is linked or not",
                        "Cleaner display for displaying user tags in dropdown list"
                    }
                 },
                { 
                    "v1.2.1", new List<string> {
                        "Dynamically render changelog"
                    }
                },
                {
                    "v1.1.0", new List<string> {
                        "Initial implementation of iCalendar App",
                        "Implement session authentication",
                        "Implement tags feature assignable for events",
                        "Ability to update user profile"
                    }
                },
                {
                    "v1.1.1", new List<string> {
                        "Change date picker selector to use by device default"
                    }
                },
                {
                    "v1.1.2", new List<string> {
                        "New scheduler feaure to be able to add multiple events in one form"
                    }
                },
                {
                    "v1.1.3", new List<string> {
                        "New scheduler feaure to be able to add multiple events in one form"
                    }
                },
                {
                    "v1.1.4", new List<string> {
                        "Use NodaTime package to manage timezones and dates",
                        "Fixes to scheduler"
                    }
                },
                {
                    "v1.1.5",  new List<string> {
                        "Ability to add multiple events by splitting them with a specified time",
                        "Use color picker instead of hard typing color name for tags"
                    }
                },
                {
                    "v1.1.6", new List<string> {
                        "Fixes when saving dates for events using NodaTime"
                    }
                },
                {
                    "v1.1.7", new List<string> {
                        "New page for showing current user activity at calendar head",
                        "Option to select the times once for all events to be added in the scheduler",
                        "Display events according to public, private or shared tag"
                    }
                },
                {
                    "v1.1.8", new List<string> {
                        "*Various fixes and style changes including new calendar theme",
                        "Fixed issue with timezone when showing current user activity",
                        "Fixes with current user activity and to tags",
                        "Notify 4 hours prior to start in current user activity",
                        "New jquery-io black styled theme for calendar",
                        "Adjust calendar content height and remove agenda in calendar header"
                    }
                },
                {
                    "v1.1.9", new List<string> {
                        "New page to show an overview for time spent in each tag with date and timespan filter",
                        "Add popover for events in the calenda"
                    }
                },
                {
                    "v1.2.0", new List<string> {
                        "*Various changes/improvements including:",
                        "Added changelog (this)",
                        "Add Cronofy API endpoint to start integrating iCalendar App with iCloud Calendar",
                        "Use cookie authentication - stay logged in for longer",
                        "Prevent editing or removing events not created by the user",
                        "Show last 7 days as default preset for events overview",
                        "Show events shared by user in your overview"
                    }
                }
            };

            return changes;
        }
    }
}
