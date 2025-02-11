<?php
/**
* @package   Warp Theme Framework
* @author    YOOtheme http://www.yootheme.com
* @copyright Copyright (C) YOOtheme GmbH
* @license   http://www.gnu.org/licenses/gpl.html GNU/GPL
*/

// no direct access
defined('_JEXEC') or die;

JHtml::_('behavior.keepalive');

?>

	<form class="short style" action="<?php echo JRoute::_('index.php', true, $params->get('usesecure')); ?>" method="post">
	
		<?php if ($params->get('pretext')) : ?>
		<div class="pretext">
			<?php echo $params->get('pretext'); ?>
		</div>
		<?php endif; ?>

		<div class="username">
			<input class="uk-width-1-1" type="text" name="username" size="18" placeholder="<?php echo JText::_('MOD_LOGIN_VALUE_USERNAME') ?>" />
		</div>

		<div class="password">
			<input class="uk-width-1-1" type="password" name="password" size="18" placeholder="<?php echo JText::_('JGLOBAL_PASSWORD') ?>" />
		</div>

		<?php if (JPluginHelper::isEnabled('system', 'remember')) : ?>
		<div class="remember">
			<?php $number = rand(); ?>
			<label for="modlgn-remember-<?php echo $number; ?>"><?php echo JText::_('MOD_LOGIN_REMEMBER_ME') ?></label>
			<input id="modlgn-remember-<?php echo $number; ?>" type="checkbox" name="remember" value="yes" checked />
		</div>
		<?php endif; ?>
		
		<div>
			<button class="button" value="<?php echo JText::_('JLOGIN') ?>" name="Submit" type="submit"><?php echo JText::_('JLOGIN') ?></button>
		</div>
		
		<ul class="blank">
			<li>
				<a href="<?php echo JRoute::_('index.php?option=com_users&view=reset'); ?>"><?php echo JText::_('MOD_LOGIN_FORGOT_YOUR_PASSWORD'); ?></a>
			</li>
			<li>
				<a href="<?php echo JRoute::_('index.php?option=com_users&view=remind'); ?>"><?php echo JText::_('MOD_LOGIN_FORGOT_YOUR_USERNAME'); ?></a>
			</li>
			<?php
			$usersConfig = JComponentHelper::getParams('com_users');
			if ($usersConfig->get('allowUserRegistration')) : ?>
			<li>
				<a href="<?php echo JRoute::_('index.php?option=com_users&view=registration'); ?>"><?php echo JText::_('MOD_LOGIN_REGISTER'); ?></a>
			</li>
			<?php endif; ?>
		</ul>
		
		<?php if($params->get('posttext')) : ?>
		<div class="posttext">
			<?php echo $params->get('posttext'); ?>
		</div>
		<?php endif; ?>
		
		<input type="hidden" name="option" value="com_users" />
		<input type="hidden" name="task" value="user.login" />
		<input type="hidden" name="return" value="<?php echo $return; ?>" />
		<?php echo JHtml::_('form.token'); ?>
	</form>
	
	<script>
		jQuery(function($){
			$('form.login input[placeholder]').placeholder();
		});
	</script>