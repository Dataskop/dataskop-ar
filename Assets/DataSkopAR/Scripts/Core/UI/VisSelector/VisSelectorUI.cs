using System.Collections.Generic;
using System.Linq;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class VisSelectorUI : MonoBehaviour {

#region Constants

		private const string GroundedClass = "grounded";
		private const string ElevatedClass = "elevated";

#endregion

#region Fields

		[Header("Events")]
		public UnityEvent<string> onAttributeSelected;
		public UnityEvent<VisualizationOption> onVisOptionSelected;

		[Header("References")]
		[SerializeField] private UIDocument visSelectorUIDoc;
		[SerializeField] private VisualTreeAsset attributeUIAsset;
		[SerializeField] private VisualTreeAsset visualizationOptionUIAsset;
		[Space(25)]
		[SerializeField] private List<Sprite> visualizationIcons = new();
		[SerializeField] private Color selectedIconColor;
		[SerializeField] private Color deselectedIconColor;
		[SerializeField] private VisualizationRepository visRepo;
		[SerializeField] private DataAttributeManager dataAttributeManager;

		private bool isStateLocked;

#endregion

#region Properties

		private VisualElement VisSelectorRoot { get; set; }

		private VisualElement VisOptionSelector { get; set; }

		private VisualElement AttributeSelector { get; set; }

		private ICollection<DataAttribute> AvailableAttributes { get; set; }

		private ICollection<VisualizationOption> AvailableVisOptions { get; set; }

		private List<Button> AttributeButtons { get; set; }

		private List<Button> VisOptionButtons { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			VisSelectorRoot = visSelectorUIDoc.rootVisualElement;
			VisOptionSelector = VisSelectorRoot.Q<VisualElement>("vis-selector");
			AttributeSelector = VisSelectorRoot.Q<VisualElement>("attribute-selector");
			dataAttributeManager.selectedAttributeChanged += SelectExternalAttribute;
		}

		private void Start() {
			VisSelectorRoot.AddToClassList(GroundedClass);
		}

		private void SelectExternalAttribute(DataAttribute selectedAttribute) {
			SelectAttributeButton(AttributeButtons.First(b => b.text == selectedAttribute.Label));
			SelectVisOptionButton(VisOptionButtons.First());
		}

		public void SetAvailableAttributes(Project projectData) {
			AttributeSelector.Clear();
			AvailableAttributes = projectData.Properties.Attributes;
			UpdateAttributeButtons();
		}

		private void UpdateAttributeButtons() {

			AttributeButtons = new List<Button>();

			foreach (DataAttribute dataAttribute in AvailableAttributes) {
				Button newAttributeButton = CreateAttributeElement(dataAttribute.ID, dataAttribute.Label);
				AttributeSelector.Add(newAttributeButton);
				AttributeButtons.Add(newAttributeButton);
			}

			if (AttributeButtons.Count > 0) SelectAttributeButton(AttributeButtons[0]);
		}

		private Button CreateAttributeElement(string attributeId, string attributeLabel) {

			TemplateContainer attributeTemplateContainer = attributeUIAsset.Instantiate();

			Button attributeButton = attributeTemplateContainer.Q<Button>("attribute");
			attributeButton.RegisterCallback<PointerDownEvent>(
				e => { UIInteractionDetection.IsPointerOverUi = true; });

			attributeButton.text = attributeLabel;
			attributeButton.name = attributeId;

			attributeButton.RegisterCallback<ClickEvent>(e => {
				SelectAttributeButton(attributeButton);
				SelectAttribute(attributeButton.name);
			});

			return attributeButton;
		}

		private void SelectAttributeButton(Button selectedButton) {

			foreach (Button b in AttributeButtons)
				if (b == selectedButton)
					b.AddToClassList("selected");
				else
					b.RemoveFromClassList("selected");

		}

		private void SelectAttribute(string attributeId) {
			onAttributeSelected?.Invoke(attributeId);
		}

		public void SetAvailableVisOptions(DataAttribute selectedAttribute) {
			VisOptionSelector.Clear();
			AvailableVisOptions = selectedAttribute.VisOptions;
			UpdateVisOptionButtons();
		}

		private void UpdateVisOptionButtons() {

			VisOptionButtons = new List<Button>();

			List<string> visNames = visRepo.GetAvailableVisualizations().Select(visType => visType.ToString()).ToList();

			foreach (VisualizationOption visOpt in AvailableVisOptions) {

				if (visNames.Contains(visOpt.Type.FirstCharToUpper()) == false) {
					visOpt.Type = visNames[0];
				}

				Button newVisOptionButton = CreateVisOptElement(visOpt);
				VisOptionButtons.Add(newVisOptionButton);
				VisOptionSelector.Add(newVisOptionButton);
			}

			if (VisOptionButtons.Count > 0) {
				SelectVisOptionButton(VisOptionButtons[0]);
			}

		}

		private Button CreateVisOptElement(VisualizationOption visOpt) {

			TemplateContainer visOptTemplateContainer = visualizationOptionUIAsset.Instantiate();

			Button visOptButton = visOptTemplateContainer.Q<Button>("opt");
			visOptButton.RegisterCallback<PointerDownEvent>(e => { UIInteractionDetection.IsPointerOverUi = true; });

			visOptButton.name = visOpt.Type;

			visOptButton.Q<VisualElement>("icon")
					.style.backgroundImage =
				new StyleBackground(visualizationIcons.First(visIcon => visIcon.name == visOpt.Type));

			visOptButton.RegisterCallback<ClickEvent>(e => {
				SelectVisOptionButton(visOptButton);
				SelectVisOption(visOpt);
			});

			return visOptButton;
		}

		private void SelectVisOptionButton(Button selectedButton) {
			foreach (Button b in VisOptionButtons)
				if (b == selectedButton) {
					b.AddToClassList("selected");
					b.Q<VisualElement>("icon").style.unityBackgroundImageTintColor = new StyleColor(selectedIconColor);
				}
				else {
					b.RemoveFromClassList("selected");
					b.Q<VisualElement>("icon").style.unityBackgroundImageTintColor =
						new StyleColor(deselectedIconColor);
				}
		}

		private void SelectVisOption(VisualizationOption visOpt) {
			onVisOptionSelected?.Invoke(visOpt);
		}

		public void ChangeVisSelectorPosition(InfoCardState infoCardState) {

			if (isStateLocked) return;

			switch (infoCardState) {
				case InfoCardState.Short:
					VisSelectorRoot.RemoveFromClassList(GroundedClass);
					VisSelectorRoot.AddToClassList(ElevatedClass);
					break;
				case InfoCardState.Collapsed:
					VisSelectorRoot.RemoveFromClassList(ElevatedClass);
					VisSelectorRoot.AddToClassList(GroundedClass);
					break;
			}

		}

		public void ChangeLockedState(bool isLocked) {
			isStateLocked = isLocked;
		}

		public void ToggleVisSelector(bool isVisible) {
			VisSelectorRoot.style.visibility =
				new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		private void OnDisable() {
			dataAttributeManager.selectedAttributeChanged -= SelectExternalAttribute;
		}

#endregion

	}

}