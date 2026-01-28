using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Notes.Models;
using Notes.Views;

namespace Notes.ViewModels;

internal class NotesViewModel : IQueryAttributable
{
    public ObservableCollection<ViewModels.NoteViewModel> AllNotes { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectNoteCommand { get; }

    public NotesViewModel()
    {
        AllNotes = new ObservableCollection<NoteViewModel>(Models.Note.LoadAll().Select(n => new NoteViewModel(n)));
        NewCommand = new AsyncRelayCommand(NewNoteAsync);
        SelectNoteCommand = new AsyncRelayCommand<NoteViewModel>(SelectNoteAsync);
    }

    private async Task SelectNoteAsync(NoteViewModel? note)
    {
        if (note != null)
            await Shell.Current.GoToAsync($"{nameof(NotePage)}?load={note.Identifier}");
    }

    private async Task NewNoteAsync()
    {
        await Shell.Current.GoToAsync(nameof(NotePage));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("deleted"))
        {
            string noteId = query["deleted"].ToString();
            NoteViewModel matchNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

            // If note exists, delete it
            if (matchNote != null)
                AllNotes.Remove(matchNote);
        }
        else if (query.ContainsKey("saved"))
        {
            string noteId = query["saved"].ToString();
            NoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

            // if note is found, update it
            if (matchedNote != null)
            {
                matchedNote.Reload();
                AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
            }

            // If note isn't found, it's new; add it
            else
                AllNotes.Insert(0, new NoteViewModel(Note.Load(noteId)));
        }
    }
}