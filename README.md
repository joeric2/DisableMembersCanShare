# DisableMembersCanShare
This solution contains a web scoped feature that will disable the ability for members of the default "Members" 
group in a SharePoint site from being able to share or add other members. It also contains an event receiver
that fires when a group is modified and IF the setting to allow group members to manage group members is set
it will set it back to false.
