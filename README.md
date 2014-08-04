Bateleur Update Mailer
======================

Bateleur Update Mailer is used to send out notification emails to all group members of a specified course when that course's website is updated. The message sent out is fixed and the member list for the course is obtained from the LDAP service running on the Commerce webserver.

Note: Created for Commerce I.T.

Created by Craig Lotter, September 2006

*********************************

Project Details:

Coded in Visual Basic .NET using Visual Studio .NET 2005
Implements concepts such as Email sending and file manipulation.
Level of Complexity: simple

*********************************

Update 20070306.04:

- Student addresses now suffixed with @uct.ac.za instead of @mail.uct.ac.za
- Silent mode enabled to affect operation complete sounds.

*********************************

Update 20071013.05: 

- Disable list during email send operation
- Added About and Help dialogs
- On error encountered, continue to try and send the other mails until user cancels operation
- Save/Load settings to Settings file.
