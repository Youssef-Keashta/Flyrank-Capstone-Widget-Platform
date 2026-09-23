namespace WidgetPlatform.Api.StaticContent;

public static class WidgetScript
{
    public const string Content = """
(function () {
  var scriptEl = document.currentScript;
  var scriptUrl = new URL(scriptEl.src);
  var apiOrigin = scriptUrl.origin;
  var widgetId = scriptUrl.searchParams.get('id');
  if (!widgetId) return;

  fetch(apiOrigin + '/api/widgets/' + widgetId + '/config')
    .then(function (res) { return res.json(); })
    .then(renderWidget)
    .catch(function () { console.error('Widget failed to load config'); });

  function renderWidget(config) {
    var container = document.createElement('div');
    container.id = 'widget-' + widgetId;

    var fields = JSON.parse(config.fieldsJson || '[]');
    var form = document.createElement('form');

    fields.forEach(function (field) {
      var label = document.createElement('label');
      label.textContent = (field.label || field.name) + ' ';
      var input = document.createElement('input');
      input.type = field.type === 'email' ? 'email' : 'text';
      input.name = field.name;
      if (field.required) input.required = true;
      label.appendChild(input);
      form.appendChild(label);
      form.appendChild(document.createElement('br'));
    });

    var submitBtn = document.createElement('button');
    submitBtn.type = 'submit';
    submitBtn.textContent = config.buttonText || 'Submit';
    form.appendChild(submitBtn);

    var resultEl = document.createElement('div');

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      var data = {};
      fields.forEach(function (field) {
        data[field.name] = form.elements[field.name].value;
      });

      fetch(apiOrigin + '/api/submissions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ widgetId: widgetId, data: data })
      })
        .then(function (res) { return res.json(); })
        .then(function () {
          resultEl.textContent = 'Thank you!';
          form.style.display = 'none';
        })
        .catch(function () {
          resultEl.textContent = 'Something went wrong.';
        });
    });

    container.appendChild(form);
    container.appendChild(resultEl);
    scriptEl.parentNode.insertBefore(container, scriptEl.nextSibling);
  }
})();
""";
}