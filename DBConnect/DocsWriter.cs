using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace DBConnect {
    public class DocsWriter : Connecter {
        private List<Course> courses;
        private int CurrentIndex;

        public DocsWriter() {
            courses = new List<Course>();
            CurrentIndex = 1;
        }

        private void ReadCourses() {
            string json = File.ReadAllText(jsonFile);
            courses = JsonConvert.DeserializeObject<List<Course>>(json);
        }

        private void AddText(string text, List<Request> requests, int index) {
            Request request = new Request();
            InsertTextRequest insertRequest = new InsertTextRequest();
            insertRequest.Text = text;

            Location location = new Location();
            location.Index = index;
            insertRequest.Location = location;

            request.InsertText = insertRequest;

            requests.Add(request);

            CurrentIndex += text.Length;
        }

        private void AddText(string text, List<Request> requests) {
            Request request = new Request();
            InsertTextRequest insertRequest = new InsertTextRequest();
            insertRequest.Text = text;
            insertRequest.EndOfSegmentLocation = new EndOfSegmentLocation();
            request.InsertText = insertRequest;

            requests.Add(request);

            CurrentIndex += text.Length;
        }

        private void AddTextLine(string text, List<Request> requests) {
            text += "\n";
            AddText(text, requests);
        }

        private void AddParagraph(string text, List<Request> requests) {
            AddTextLine(text, requests);

            Request request = new Request();
            UpdateParagraphStyleRequest updateParagraphStyleRequest = new UpdateParagraphStyleRequest();
            Google.Apis.Docs.v1.Data.Range range = new Google.Apis.Docs.v1.Data.Range();
            range.StartIndex = CurrentIndex - 1 - text.Length;
            range.EndIndex = CurrentIndex;
            ParagraphStyle paragraphStyle = new ParagraphStyle();
            paragraphStyle.NamedStyleType = "NORMAL_TEXT";

            updateParagraphStyleRequest.Range = range;
            updateParagraphStyleRequest.ParagraphStyle = paragraphStyle;
            updateParagraphStyleRequest.Fields = "*";
            request.UpdateParagraphStyle = updateParagraphStyleRequest;

            requests.Add(request);
        }

        private void AddHeaderLine(string text, List<Request> requests, int header, bool newline = true) {
            if ( newline ) {
                AddTextLine(text, requests);
            } else {
                AddText(text, requests);
            }

            Request request = new Request();
            UpdateParagraphStyleRequest updateParagraphStyleRequest = new UpdateParagraphStyleRequest();
            Google.Apis.Docs.v1.Data.Range range = new Google.Apis.Docs.v1.Data.Range();

            if (newline) {
                range.StartIndex = CurrentIndex - 1 - text.Length;
            } else {
                range.StartIndex = CurrentIndex - text.Length;
            }
            range.EndIndex = CurrentIndex;
            ParagraphStyle paragraphStyle = new ParagraphStyle();
            switch (header) {
                case 1:
                    paragraphStyle.NamedStyleType = "HEADING_1";
                    break;
                case 2:
                    paragraphStyle.NamedStyleType = "HEADING_2";
                    break;
                case 3:
                    paragraphStyle.NamedStyleType = "HEADING_3";
                    break;
                default:
                    throw new Exception("Invalid header");
            }

            updateParagraphStyleRequest.Range = range;
            updateParagraphStyleRequest.ParagraphStyle = paragraphStyle;
            updateParagraphStyleRequest.Fields = "*";
            request.UpdateParagraphStyle = updateParagraphStyleRequest;

            requests.Add(request);
        }

        private void Underline(int startIndex, int endIndex, List<Request> requests) {
            Request request = new Request();

            UpdateTextStyleRequest updateTextStyleRequest = new UpdateTextStyleRequest();
            TextStyle textStyle = new TextStyle();
            textStyle.Underline = true;
            updateTextStyleRequest.TextStyle = textStyle;
            updateTextStyleRequest.Fields = "*";

            Google.Apis.Docs.v1.Data.Range range = new Google.Apis.Docs.v1.Data.Range();
            range.StartIndex = startIndex;
            range.EndIndex = endIndex;

            updateTextStyleRequest.Range = range;
            request.UpdateTextStyle = updateTextStyleRequest;

            requests.Add(request);
        }

        private void AddQuizQuestions(QuizLevel quizLevel, List<Request> requests) {
            Request request = new Request();

            InsertTableRequest insertTableRequest = new InsertTableRequest();
            insertTableRequest.Columns = 2;
            insertTableRequest.Rows = quizLevel.questions.Count;
            insertTableRequest.EndOfSegmentLocation = new EndOfSegmentLocation();
            
            request.InsertTable = insertTableRequest;

            requests.Add(request);

            int insertIndex = CurrentIndex;
            insertIndex += 4;

            foreach(Question question in quizLevel.questions) {
                AddText(question.question, requests, insertIndex);
                insertIndex += question.question.Length;
                insertIndex += 2;

                CreateParagraphBulletsRequest createParagraphBulletsRequest = new CreateParagraphBulletsRequest();
                createParagraphBulletsRequest.BulletPreset = "BULLET_DISC_CIRCLE_SQUARE";
                Google.Apis.Docs.v1.Data.Range range = new Google.Apis.Docs.v1.Data.Range();
                range.StartIndex = insertIndex;

                int n = question.possibleAnswers.Count;
                for (int i = 0; i < n; i ++) {
                    string text = question.possibleAnswers[i].answer;
                    if ( i != n - 1 ) {
                        text += "\n";
                    }
                    AddText(text, requests, insertIndex);

                    if ( question.possibleAnswers[i].answer == question.correct.answer ) {
                        Underline(insertIndex, insertIndex + question.possibleAnswers[i].answer.Length, requests);
                    }

                    insertIndex += text.Length;
                }

                range.EndIndex = insertIndex;
                createParagraphBulletsRequest.Range = range;
                request = new Request();
                request.CreateParagraphBullets = createParagraphBulletsRequest;
                requests.Add(request);

                insertIndex += 3;
            }

            
            CurrentIndex += 6;
            CurrentIndex += quizLevel.questions.Count * 3;
            CurrentIndex += (quizLevel.questions.Count - 1) * 2;
        }

        private void DeleteAllContent(List<Request> requests, Document doc, DocsService service, string documentId) {
            DeleteContentRangeRequest deleteContentRangeRequest = new DeleteContentRangeRequest();

            Google.Apis.Docs.v1.Data.Range range = new Google.Apis.Docs.v1.Data.Range();
            range.StartIndex = 1;
            range.EndIndex = 1000000000;

            deleteContentRangeRequest.Range = range;

            Request request = new Request();
            request.DeleteContentRange = deleteContentRangeRequest;

            try {
                BatchUpdateDocumentRequest tryBody = new BatchUpdateDocumentRequest();

                tryBody.Requests = new List<Request> { request };
                DocumentsResource.BatchUpdateRequest tryUpdateRequest = service.Documents.BatchUpdate(tryBody, documentId);

                BatchUpdateDocumentResponse tryResponse = tryUpdateRequest.Execute();
            } catch ( Exception e ) {
                string[] responseData = e.Message.Split("must be less than the end index of the referenced segment, ");
                responseData = responseData[1].Split(".");

                range.EndIndex = Int32.Parse(responseData[0]) - 1;

                if ( range.EndIndex - range.StartIndex != 0 ) {
                    deleteContentRangeRequest.Range = range;

                    request = new Request();
                    request.DeleteContentRange = deleteContentRangeRequest;
                    requests.Add(request);
                }
            }
        }

        private void Write() {
            string[] Scopes = { DocsService.Scope.Documents, DocsService.Scope.DriveFile, DocsService.Scope.Drive };

            UserCredential credential;

            using (var stream = new FileStream($"/Users/mathiasgammelgaard/credentials/credentials.json", FileMode.Open, FileAccess.Read)) {
                string credPath = "/Users/mathiasgammelgaard/credentials/token.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            DocsService service = new DocsService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential
            });

            // Define request parameters.
            String documentId = "1tldY_DpX5IM9wPqIKN9FVgwfBT2Ueq__F8pwOI8dLVI";
            DocumentsResource.GetRequest documentRequest = service.Documents.Get(documentId);
            Document doc = documentRequest.Execute();

            List<Request> requests = new List<Request>();
            DeleteAllContent(requests, doc, service, documentId);
            
            foreach (Course course in courses) {
                //AddHeaderLine(course.title, requests, 1);
                foreach(Section section in course.sections) {
                    if (section.description != null ) {
                        AddHeaderLine(section.name, requests, 3);
                        AddParagraph(section.description.levels[0].description, requests);
                    } else {
                        AddHeaderLine(section.name, requests, 3, false);
                        AddQuizQuestions(section.quiz.levels[0], requests);
                    }
                }
            }

            

            BatchUpdateDocumentRequest body = new BatchUpdateDocumentRequest();

            body.Requests = requests;
            DocumentsResource.BatchUpdateRequest updateRequest = service.Documents.BatchUpdate(body, documentId);

            updateRequest.Execute();
        }

        public void run() {
            connect();
            ReadCourses();
            Write();
            CloseConnection();
        }
    }
}
