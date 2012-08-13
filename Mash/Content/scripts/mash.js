/**
 * mash.js
 *
 *   Set up a *mash environment
 *
 * Licensed under the MIT:
 * http://www.opensource.org/licenses/mit-license.php
 *
 * @author Pat Newell (@pnewell4)
 * @version 0.1
 * @link 
*/

;(function($, document, window, undefined) {
	var CONSTANTS = {
		DATAKEY_MEDIA: '$media'
	};

	// this is the magic, call $jqueryObj.mash() with either a method or 
	$.fn.mash = function(method) {
		if (methods[method]) {
			return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		} else if (typeof method === 'object' || ! method) {
			return methods.init.apply(this, arguments);
		} else {
			$.error('Method ' +  method + ' does not exist on jQuery.GCProfile');
		}
		return null;
	};

	$.fn.mash.defaults = {
		debug: true,
		mashSelector: ".mash-display",
		mediaWrapper: ".media-content",
		mediaSelector: ".media",
		submitButton: "input[type='submit'], button[type='submit']",
		completeClassname: "mashed",    // gets added to mashSelector when complete
		winnerClassname: "mash-winner", // gets added to the winning mediaSelector
		losersClassname: "mash-losers"  // gets added to all losing mediaSelector(s)
	};

	var methods = {
		init: function(options) {
			var options = $.extend({}, $.fn.mash.defaults, options);
			
			return this.each(function() {
				// Set up basic vars
				var o = options,
				    $this = $(this);

				// Start Plugin here
				$this.on({
					click: function(){
						$(o.submitButton, $(this)).trigger("click");
					}
				}, o.mediaWrapper);
				$this.on({
					mashSetup: function(){
						var $this = $(this),
						    portrait = $this.height() > $this.width()
						    $parent = $this.closest(o.mashSelector),
						    $window = $(window),
						    containerWidth = $window.width(),
						    mediaWidth = Math.min(containerWidth * 0.9/2), // should check to see how many media items there are...
						    containerHeight = $window.height(),
						    mediaMax = Math.min(mediaWidth, containerHeight);

						$this.css({
							"max-width": mediaMax + "px",
							"max-height": mediaMax + "px",
							width: "auto",
							height: "auto"
						}).parent().css({
							width: mediaMax + "px",
							height: mediaMax + "px"
						});
					}
				}, o.mediaSelector);
				$this.on({
					click :function(e){
						var $this = $(this),
						    $form = $this.closest("form"),
						    formData = $form.serialize() || "";

						if(!$this.closest(o.mashSelector).length)
							return;

						e.stopPropagation();

						if(formData.length)
							formData += '&';
						formData += $this.attr("name") + "=" + $this.val();

						$.ajax({
							type: "post",
							url: $form.attr("action"),
							data: formData,
							success: function(){
								var $winner = $this.closest(o.mediaWrapper),
								    $mash = $this.closest(o.mashSelector),
								    $losers = $(o.mediaWrapper, $mash).not($winner);

								$winner.addClass(o.winnerClassname);
								$losers.addClass(o.losersClassname);
								$mash.addClass(o.completeClassname);
							}
						});

						return false;
					}
				}, o.submitButton);

				$(o.mediaSelector, $this).trigger("mashSetup"); // since these have probably already loadaed
			});
		}
	};
}) (jQuery, document, window);
