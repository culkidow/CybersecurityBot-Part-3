using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CybersecurityBot
{
    public class MainForm : Form
    {
        private readonly ChatBotEngine _botEngine = new();
        private readonly TaskStorage _storage = new();
        private readonly ActivityLogger _activity = new();
        private bool _dbReady;
        private TabControl tabs = new();
        private RichTextBox chatHistory = new();
        private TextBox chatInput = new();
        private ListView taskList = new();
        private TextBox taskTitle = new();
        private TextBox taskDescription = new();
        private CheckBox hasReminder = new();
        private DateTimePicker reminderDate = new();
        private Label dbStatus = new();
        private Label quizQuestion = new();
        private RadioButton[] quizOptions = new RadioButton[4];
        private Button quizNext = new();
        private List<QuizQuestion> quiz = new();
        private int quizIndex, quizScore;
        private bool quizFinished;

        public MainForm()
        {
            Text = "WEchat Cybersecurity Assistant | Part 3";
            Size = new Size(1000, 760);
            MinimumSize = new Size(850, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            BuildInterface();
            try { _storage.Initialize(); _dbReady = true; dbStatus.Text = "MySQL: Connected"; dbStatus.ForeColor = Color.DarkGreen; RefreshTasks(); }
            catch (Exception ex) { dbStatus.Text = "MySQL: Not connected — configure password in TaskStorage.cs"; dbStatus.ForeColor = Color.DarkRed; }
            _activity.Log("Application started");
            AppendChat("WEchat Bot", "Welcome to WEchat Cybersecurity Assistant! Part 3 features: task assistant, quiz, NLP command recognition and activity log. First, may I please have your name?");
        }

        private void BuildInterface()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.FromArgb(192,0,0) };
            header.Controls.Add(new Label { Text = "WEchat  |  CYBERSECURITY ASSISTANT", ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            tabs.Dock = DockStyle.Fill;
            tabs.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            tabs.TabPages.Add(BuildChatTab());
            tabs.TabPages.Add(BuildTasksTab());
            tabs.TabPages.Add(BuildQuizTab());
            tabs.TabPages.Add(BuildActivityTab());
            Controls.Add(tabs); Controls.Add(header);
        }

        private TabPage BuildChatTab()
        {
            var page = new TabPage("Chat / NLP");
            chatHistory.Dock = DockStyle.Fill; chatHistory.ReadOnly = true; chatHistory.Font = new Font("Segoe UI", 11);
            chatHistory.BackColor = Color.FromArgb(250,250,250);
            var inputPanel = new Panel { Dock = DockStyle.Bottom, Height = 55 };
            chatInput.Location = new Point(12,12); chatInput.Width = 790; chatInput.Font = new Font("Segoe UI", 11);
            chatInput.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            chatInput.KeyDown += (s,e) => { if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true; SendChat();} };
            var send = new Button { Text="Send", Location=new Point(815,9), Size=new Size(120,35), BackColor=Color.FromArgb(192,0,0), ForeColor=Color.White, Anchor=AnchorStyles.Right|AnchorStyles.Top };
            send.Click += (s,e)=>SendChat();
            inputPanel.Controls.Add(chatInput); inputPanel.Controls.Add(send);
            var hint = new Label { Text="Try: “add task review privacy settings”, “show tasks”, “start quiz”, “show activity log”", Dock=DockStyle.Top, Height=30, ForeColor=Color.DimGray, TextAlign=ContentAlignment.MiddleLeft };
            page.Controls.Add(chatHistory); page.Controls.Add(inputPanel); page.Controls.Add(hint);
            return page;
        }

        private TabPage BuildTasksTab()
        {
            var page = new TabPage("Task Assistant");
            var top = new Panel { Dock=DockStyle.Top, Height=190, Padding=new Padding(12) };
            taskTitle = new TextBox { Location=new Point(12,12), Width=400, PlaceholderText="Task title (e.g., Enable two-factor authentication)" };
            taskDescription = new TextBox { Location=new Point(12,48), Width=600, Height=48, Multiline=true, PlaceholderText="Description / details" };
            hasReminder = new CheckBox { Text="Set reminder", Location=new Point(12,110), AutoSize=true };
            reminderDate = new DateTimePicker { Location=new Point(125,106), Width=260, Format=DateTimePickerFormat.Custom, CustomFormat="yyyy-MM-dd HH:mm", ShowUpDown=true, Value=DateTime.Now.AddDays(1) };
            var add = new Button { Text="Add task", Location=new Point(12,145), Width=100 };
            var done = new Button { Text="Mark completed", Location=new Point(125,145), Width=140 };
            var del = new Button { Text="Delete task", Location=new Point(275,145), Width=110 };
            add.Click += (s,e)=>AddTaskFromFields();
            done.Click += (s,e)=>ChangeSelectedTask(true);
            del.Click += (s,e)=>DeleteSelectedTask();
            top.Controls.AddRange(new Control[]{taskTitle,taskDescription,hasReminder,reminderDate,add,done,del});
            taskList.Dock=DockStyle.Fill; taskList.View=View.Details; taskList.FullRowSelect=true; taskList.GridLines=true;
            taskList.Columns.Add("ID",55); taskList.Columns.Add("Task",250); taskList.Columns.Add("Description",350); taskList.Columns.Add("Reminder",170); taskList.Columns.Add("Status",100);
            dbStatus.Dock=DockStyle.Bottom; dbStatus.Height=30; dbStatus.TextAlign=ContentAlignment.MiddleLeft;
            page.Controls.Add(taskList); page.Controls.Add(top); page.Controls.Add(dbStatus);
            return page;
        }

        private TabPage BuildQuizTab()
        {
            var page = new TabPage("Cybersecurity Quiz");
            var start = new Button { Text="Start / Restart Quiz", Location=new Point(20,20), Size=new Size(180,38) };
            start.Click += (s,e)=>StartQuiz();
            quizQuestion = new Label { Location=new Point(20,80), Size=new Size(900,90), Font=new Font("Segoe UI",14,FontStyle.Bold), Text="Press Start to begin the cybersecurity quiz." };
            for(int i=0;i<4;i++){ quizOptions[i]=new RadioButton { Location=new Point(35,180+i*48), Width=850, Font=new Font("Segoe UI",11), Enabled=false }; }
            quizNext = new Button { Text="Submit answer / Next", Location=new Point(20,390), Size=new Size(190,40), Enabled=false };
            quizNext.Click += (s,e)=>NextQuiz();
            page.Controls.Add(start); page.Controls.Add(quizQuestion); page.Controls.AddRange(quizOptions); page.Controls.Add(quizNext);
            return page;
        }

        private TabPage BuildActivityTab()
        {
            var page=new TabPage("Activity Log");
            var list=new ListBox { Dock=DockStyle.Fill, Font=new Font("Consolas",10) };
            var refresh=new Button { Text="Refresh (last 10 actions)", Dock=DockStyle.Top, Height=40 };
            refresh.Click += (s,e)=>{ list.Items.Clear(); list.Items.AddRange(_activity.GetRecent(10).ToArray()); };
            page.Controls.Add(list); page.Controls.Add(refresh);
            page.Enter += (s,e)=>{list.Items.Clear();list.Items.AddRange(_activity.GetRecent(10).ToArray());};
            return page;
        }

        private void SendChat()
        {
            var input=chatInput.Text.Trim(); if(input.Length==0)return;
            AppendChat("You",input); chatInput.Clear();
            string lower=input.ToLowerInvariant();
            string response;
            if(ContainsAny(lower,"show tasks","view tasks","list tasks","my tasks"))
            { response=ShowTasksText(); _activity.Log("NLP command: viewed task list"); }
            else if(ContainsAny(lower,"activity log","what have you done","show log","what did you do"))
            { response="Recent actions:\\n"+string.Join("\\n",_activity.GetRecent(10)); _activity.Log("NLP command: viewed activity log"); }
            else if(ContainsAny(lower,"start quiz","begin quiz","cybersecurity quiz","play quiz"))
            { tabs.SelectedTab=tabs.TabPages[2]; StartQuiz(); response="The cybersecurity quiz has started. Answer the question shown in the Quiz tab."; _activity.Log("NLP command: started quiz"); }
            else if(ContainsAny(lower,"add task","create task","new task","remind me"))
            {
                string title=ExtractTaskTitle(input);
                if(title.Length>0){taskTitle.Text=title; taskDescription.Text="Cybersecurity task added through chatbot NLP command."; tabs.SelectedTab=tabs.TabPages[1]; response=$"I identified the task “{title}”. It is entered in the Task Assistant. Click Add task to save it, and optionally set a reminder."; _activity.Log("NLP recognized task request: "+title);}
                else response="I can help add a task. Please phrase it like: “Add task review privacy settings”.";
            }
            else
            { response=_botEngine.ProcessMessage(input); _activity.Log("Chat/NLP interaction: "+input); }
            AppendChat("WEchat Bot",response);
        }

        private static bool ContainsAny(string text, params string[] phrases)=>phrases.Any(p=>text.Contains(p));
        private static string ExtractTaskTitle(string input)
        {
            string lower=input.ToLowerInvariant();
            foreach(string phrase in new[]{"add task","create task","new task","remind me to","remind me"})
            { int idx=lower.IndexOf(phrase); if(idx>=0){string title=input.Substring(idx+phrase.Length).Trim(' ',':','-','.'); if(title.Length>0)return char.ToUpper(title[0])+title.Substring(1);}}
            return "";
        }

        private void AddTaskFromFields()
        {
            if(!_dbReady){MessageBox.Show("MySQL is not connected. Configure the connection string in TaskStorage.cs and restart.", "Database required");return;}
            if(string.IsNullOrWhiteSpace(taskTitle.Text)){MessageBox.Show("Enter a task title.");return;}
            try{_storage.Add(taskTitle.Text.Trim(),taskDescription.Text.Trim(),hasReminder.Checked?(DateTime?)reminderDate.Value:null);_activity.Log("Task added: "+taskTitle.Text.Trim());taskTitle.Clear();taskDescription.Clear();hasReminder.Checked=false;RefreshTasks();}
            catch(Exception ex){MessageBox.Show("Could not save task: "+ex.Message);}
        }
        private void RefreshTasks()
        {
            if(!_dbReady)return;
            try{taskList.Items.Clear();foreach(var t in _storage.GetAll()){var item=new ListViewItem(t.Id.ToString());item.SubItems.Add(t.Title);item.SubItems.Add(t.Description);item.SubItems.Add(t.ReminderAt?.ToString("yyyy-MM-dd HH:mm")??"—");item.SubItems.Add(t.IsCompleted?"Completed":"Pending");taskList.Items.Add(item);}}
            catch(Exception ex){MessageBox.Show("Could not load tasks: "+ex.Message);}
        }
        private int? SelectedTaskId()=>taskList.SelectedItems.Count==0?null:int.Parse(taskList.SelectedItems[0].Text);
        private void ChangeSelectedTask(bool done){var id=SelectedTaskId();if(id==null){MessageBox.Show("Select a task first.");return;}try{_storage.SetCompleted(id.Value,done);_activity.Log("Task marked completed: ID "+id);RefreshTasks();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void DeleteSelectedTask(){var id=SelectedTaskId();if(id==null){MessageBox.Show("Select a task first.");return;}try{_storage.Delete(id.Value);_activity.Log("Task deleted: ID "+id);RefreshTasks();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private string ShowTasksText(){if(!_dbReady)return "The task database is not connected. Please configure MySQL.";try{var tasks=_storage.GetAll();return tasks.Count==0?"You have no saved tasks.":"Your tasks:\\n"+string.Join("\\n",tasks.Select(t=>$"{t.Id}. {t.Title} — {(t.IsCompleted?"Completed":"Pending")}"+(t.ReminderAt.HasValue?$" (Reminder: {t.ReminderAt:yyyy-MM-dd HH:mm})":"")));}catch{return "I could not retrieve tasks from the database.";}}
        private void AppendChat(string who,string msg){chatHistory.SelectionStart=chatHistory.TextLength;chatHistory.SelectionColor=who=="You"?Color.FromArgb(40,40,40):Color.FromArgb(192,0,0);chatHistory.SelectionFont=new Font("Segoe UI",10,FontStyle.Bold);chatHistory.AppendText(who+": ");chatHistory.SelectionColor=Color.Black;chatHistory.SelectionFont=new Font("Segoe UI",10);chatHistory.AppendText(msg+Environment.NewLine+Environment.NewLine);chatHistory.ScrollToCaret();}

        private void StartQuiz()
        {
            quiz=new List<QuizQuestion>{
                Q("What should you do if an email asks for your password?",new[]{"Reply with password","Delete only","Report as phishing","Forward to friends"},2,"Never disclose passwords; report suspicious messages."),
                Q("A strong password should ideally be…",new[]{"123456","Unique and long","Your birthday","Same on every site"},1,"Use long, unique passwords for each account."),
                Q("What does HTTPS indicate?",new[]{"Encrypted connection to site","Site is always trustworthy","No malware possible","A free account"},0,"HTTPS encrypts traffic, but does not guarantee a site is legitimate."),
                Q("What is two-factor authentication?",new[]{"Two usernames","A second verification step","Sharing password twice","A firewall setting"},1,"2FA adds an additional proof of identity."),
                Q("True or False: You should click unexpected email attachments.",new[]{"True","False","Only at night","Always"},1,"Unexpected attachments can contain malware."),
                Q("What is phishing?",new[]{"A backup method","A scam to steal data","A software update","A safe browser"},1,"Phishing impersonates trusted sources to steal information."),
                Q("Why install security updates?",new[]{"Change wallpaper","Patch vulnerabilities","Remove all passwords","Speed internet only"},1,"Updates patch known security weaknesses."),
                Q("Public Wi-Fi is safest when…",new[]{"Sharing banking details freely","Using secured connections and VPN when appropriate","Turning off screen lock","Using same password"},1,"Avoid sensitive activity on untrusted networks."),
                Q("What is social engineering?",new[]{"Manipulating people to reveal information","Building a website","Encrypting a drive","Updating apps"},0,"Attackers manipulate human trust."),
                Q("True or False: A password manager can help create unique passwords.",new[]{"True","False","Only for email","Never"},0,"Password managers store and generate unique credentials."),
                Q("What should you do with an unknown USB drive?",new[]{"Plug it in immediately","Give it to a friend","Do not connect; report it","Format your PC"},2,"Unknown USB devices can carry malware."),
                Q("Which is a good backup practice?",new[]{"One copy only on PC","Regular backups in a separate secure location","Never backup","Use public computer"},1,"Keep regular backups separated from primary data.")
            };
            quizIndex=0;quizScore=0;quizFinished=false;_activity.Log("Quiz started (12 questions)");DisplayQuizQuestion();
        }
        private QuizQuestion Q(string q,string[] options,int correct,string explanation)=>new(){Question=q,Options=options,CorrectIndex=correct,Explanation=explanation};
        private void DisplayQuizQuestion(){if(quizIndex>=quiz.Count){quizFinished=true;quizQuestion.Text=$"Quiz complete! Score: {quizScore}/{quiz.Count}. "+(quizScore>=9?"Great job! You're a cybersecurity pro!": "Keep learning to stay safe online!");foreach(var r in quizOptions)r.Enabled=false;quizNext.Enabled=false;_activity.Log($"Quiz completed: score {quizScore}/{quiz.Count}");return;}var q=quiz[quizIndex];quizQuestion.Text=$"Question {quizIndex+1} of {quiz.Count}\\n{q.Question}";for(int i=0;i<4;i++){quizOptions[i].Text=$"{(char)('A'+i)}) {q.Options[i]}";quizOptions[i].Checked=false;quizOptions[i].Enabled=true;}quizNext.Text="Submit answer / Next";quizNext.Enabled=true;}
        private void NextQuiz(){if(quizFinished)return;int selected=Array.FindIndex(quizOptions,r=>r.Checked);if(selected<0){MessageBox.Show("Choose an answer first.");return;}var q=quiz[quizIndex];bool correct=selected==q.CorrectIndex;if(correct)quizScore++;MessageBox.Show((correct?"Correct! ":"Not quite. ")+q.Explanation,"Quiz feedback");_activity.Log($"Quiz answer {quizIndex+1}: {(correct?"correct":"incorrect")}");quizIndex++;DisplayQuizQuestion();}
    }
}
