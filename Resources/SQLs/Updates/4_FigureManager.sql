
CREATE TABLE `figure_sets` (
    `id` INT(10) NOT NULL,
    `type` VARCHAR(5) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
    `palette_id` INT(10) NOT NULL,
    `gender` ENUM('m','f','u') NOT NULL DEFAULT 'u' COLLATE 'utf8mb4_0900_ai_ci',
    `club_level` INT(10) NOT NULL,
     `colorable` TINYINT(1) NOT NULL DEFAULT '0',
     `selectable` TINYINT(1) NOT NULL DEFAULT '0',
     `pre_selectable` TINYINT(1) NOT NULL DEFAULT '0',
     PRIMARY KEY (`id`) USING BTREE,
     UNIQUE INDEX `type` (`type`, `palette_id`, `id`) USING BTREE
)
    COLLATE='utf8mb4_0900_ai_ci'
    ENGINE=InnoDB
;

CREATE TABLE `figure_set_colors` (
    `id` INT(10) NOT NULL,
    `palette_id` INT(10) NOT NULL,
    `index` INT(10) NOT NULL,
    `club_level` INT(10) NOT NULL,
    `selectable` TINYINT(1) NOT NULL DEFAULT '0',
    `value` VARCHAR(10) NOT NULL COLLATE 'utf8mb4_0900_ai_ci',
     PRIMARY KEY (`id`) USING BTREE
)
    COLLATE='utf8mb4_0900_ai_ci'
    ENGINE=InnoDB
;

CREATE TABLE `figure_set_palettes` (
    `id` INT(10) NOT NULL,
    `color_id` INT(10) NOT NULL,
    UNIQUE INDEX `id` (`id`, `color_id`) USING BTREE,
    INDEX `color` (`color_id`) USING BTREE
)
    COLLATE='utf8mb4_0900_ai_ci'
    ENGINE=InnoDB
;
