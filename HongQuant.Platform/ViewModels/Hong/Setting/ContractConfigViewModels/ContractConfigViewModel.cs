using HongQuant.Core.Models;
using HongQuant.Platform.Infrastucture;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels
{
    public class ContractConfigViewModel : ContractCollectionBase
    {
        [Reactive]
        public string InputTypeName { get; set; } = string.Empty;

        [Reactive]
        public bool IsSortContractDialogOpen { get; set; }

        [Reactive]
        public object? Model { get; set; }

        public ICommand AddTypeCommand { get; }

        public ICommand RemoveTypeCommand { get; }

        public ICommand EditTypeCommand { get; }

        public ICommand TypeEditSubmitCommand { get; }

        public ICommand AddContractCommand { get; }

        public ICommand AddOrEditContractSubmitCommand { get; }

        public ICommand EditContractCommand { get; }

        public ICommand RemoveContractCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand ImportCommand { get; }

        public ContractConfigViewModel() : base()
        {
            var canExecute = this.WhenAnyValue(
                x => x.InputTypeName,
                (typeName) => !string.IsNullOrEmpty(typeName));

            AddTypeCommand = ReactiveCommand.Create(AddType, canExecute);
            RemoveTypeCommand = ReactiveCommand.CreateFromTask<ContractTypeViewModel>(RemoveTypeAsync);
            EditTypeCommand = ReactiveCommand.Create<ContractTypeViewModel>(EditType);
            TypeEditSubmitCommand = ReactiveCommand.Create<EditContractTypeViewModel>(TypeEditSubmit);
            AddContractCommand = ReactiveCommand.Create<ContractTypeViewModel>(AddContract);
            AddOrEditContractSubmitCommand = ReactiveCommand.Create<CreateOrEditContractViewModel>(AddOrEditContractSubmit);
            EditContractCommand = ReactiveCommand.Create<ContractViewModel>(EditContract);
            RemoveContractCommand = ReactiveCommand.CreateFromTask<ContractViewModel>(RemoveContractAsync);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportAsync);

            ((ReactiveCommand<CreateOrEditContractViewModel, Unit>)AddOrEditContractSubmitCommand)
                .ThrownExceptions
                .Subscribe(async ex => await Interactions.HongSettingError.Handle(ex.Message));
        }

        private void AddType()
        {
            _hongContractConfigService.AddType(InputTypeName);
        }

        private async Task RemoveTypeAsync(ContractTypeViewModel typeViewModel)
        {
            var delete = await Interactions.HongSettingConfirm.Handle($"板块{typeViewModel.Name}");

            if (delete)
            {
                _hongContractConfigService.RemoveType(typeViewModel.Id);
            }
        }

        private void EditType(ContractTypeViewModel typeViewModel)
        {
            IsSortContractDialogOpen = true;
            Model = new EditContractTypeViewModel(typeViewModel.Id, typeViewModel.Name);
        }

        private void TypeEditSubmit(EditContractTypeViewModel typeViewModel)
        {
            IsSortContractDialogOpen = false;
            Model = null;

            _hongContractConfigService.UpdateType(typeViewModel.Id, typeViewModel.Name);
        }

        private void AddContract(ContractTypeViewModel typeViewModel)
        {
            IsSortContractDialogOpen = true;
            Model = new CreateOrEditContractViewModel()
            {
                TypeId = typeViewModel.Id,
            };
        }

        private void AddOrEditContractSubmit(CreateOrEditContractViewModel viewModel)
        {
            IsSortContractDialogOpen = false;
            Model = null;

            var name = new ContractName(viewModel.DefaultName, viewModel.OtherNames);

            if (Guid.Empty == viewModel.Id)
            {
                _hongContractConfigService.AddContract(viewModel.TypeId, viewModel.Key, name);
            }
            else
            {
                _hongContractConfigService.EditContract(viewModel.TypeId, viewModel.Id, viewModel.Key, name);
            }

        }

        private async Task RemoveContractAsync(ContractViewModel viewModel)
        {
            var delete = await Interactions.HongSettingConfirm.Handle($"品种{viewModel.Key}-{viewModel.Name}");

            if (delete)
            {
                _hongContractConfigService.RemoveContract(viewModel.TypeId, viewModel.Id);
            }
        }

        private void EditContract(ContractViewModel viewModel)
        {
            IsSortContractDialogOpen = true;
            Model = new CreateOrEditContractViewModel(viewModel.OtherNames)
            {
                Id = viewModel.Id,
                TypeId = viewModel.TypeId,
                Key = viewModel.Key,
                DefaultName = viewModel.Name,
            };
        }

        private async Task ExportAsync()
        {
            var fileName = await Interactions.HongSettingSaveFileDialog.Handle("Contract.json");

            if (string.IsNullOrEmpty(fileName)) return;

            var types = _hongContractConfigService.ContractTypes.Items;

            var json = JsonConvert.SerializeObject(types, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new LimitPropsContractResolver(["Id"], false)
            });

            File.WriteAllText(fileName, json);
        }

        private async Task ImportAsync()
        {
            var fileName = await Interactions.HongSettingOpenFileDialog.Handle("");

            if (string.IsNullOrEmpty(fileName)) return;

            var json = File.ReadAllText(fileName);

            var types = JsonConvert.DeserializeObject<List<ContractType>>(json, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new LimitPropsContractResolver(["Id"], false)
            });

            if (types != null)
            {
                foreach (var type in types)
                {
                    type.Id = Guid.NewGuid();
                    foreach (var contract in type.Contracts)
                    {
                        contract.Id = Guid.NewGuid();
                        contract.ContractType = type;
                    }
                }
                _hongContractConfigService.ReplaceTypes(types);
            }

        }
    }
}
